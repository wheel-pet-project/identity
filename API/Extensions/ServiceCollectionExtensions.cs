using Application.Infrastructure.Interfaces.EmailProvider;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Application.UseCases.CreateAccount;
using Application.UseCases.Interfaces;
using Ardalis.SmartEnum.Dapper;
using Dapper;
using Infrastructure.EmailProvider;
using Infrastructure.PasswordHasher;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Settings;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSqlMapperForEnums()
    {
        SqlMapper.AddTypeHandler(typeof(Role), new SmartEnumByValueTypeHandler<Domain.AccountAggregate.Role>());
        SqlMapper.AddTypeHandler(typeof(Status), new SmartEnumByValueTypeHandler<Domain.AccountAggregate.Status>());
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<CreateAccountRequest, CreateAccountResponse>, CreateAccountUseCase>();
        
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IAccountRepository, AccountRepository>();
        
        return services;
    }

    public static IServiceCollection AddPasswordHasher(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        
        return services;
    }

    public static IServiceCollection AddEmailProvider(this IServiceCollection services)
    {
        services.AddTransient<IEmailProvider, EmailProvider>();
        
        return services;
    }
    
    public static IServiceCollection ConfigureFluentEmail(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

        services.AddFluentEmail(emailSettings!.DefaultFromEmail)
            .AddSmtpSender(emailSettings.SmtpHost, emailSettings.SmtpPort);
        
        return services;
    }
    
    public static IServiceCollection ConfigureSerilog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .CreateLogger();
        services.AddSerilog();
        
        return services;
    }
    
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddPrometheusExporter();

                builder.AddMeter("Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel");
                builder.AddView("http.server.request.duration",
                    new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries =
                        [
                            0, 0.005, 0.01, 0.025, 0.05,
                            0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
                        ]
                    });
            })
            .WithTracing(builder =>
            {
                builder
                    .AddGrpcClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddNpgsql()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("Identity"))
                    .AddSource("Identity")
                    // .AddSource("MassTransit") 
                    .AddJaegerExporter();
            });;
        
        return services;
    }
}