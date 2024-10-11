using System.Data;
using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.JwtProvider;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Application.UseCases.Account.Authenticate;
using Application.UseCases.Account.Authorize;
using Application.UseCases.Account.Create;
using Ardalis.SmartEnum.Dapper;
using Dapper;
using Domain.AccountAggregate;
using Infrastructure.JwtProvider;
using Infrastructure.PasswordHasher;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Settings;
using MassTransit;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Status = Domain.AccountAggregate.Status;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbConnection(this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection("ConnectionStrings")
            .Get<DbConnectionSettings>();
        services.AddScoped<IDbConnection>(_ => 
            new NpgsqlConnection(dbSettings!.ConnectionString));

        return services;
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<CreateAccountRequest, CreateAccountResponse>, 
            CreateAccountUseCase>();
        services.AddScoped<IUseCase<AuthenticateAccountRequest, AuthenticateAccountResponse>,
            AuthenticateAccountUseCase>();
        services.AddScoped<IUseCase<AuthorizeAccountRequest, AuthorizeAccountResponse>,
            AuthorizeAccountUseCase>();
        
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IAccountRepository, AccountRepository>();
        
        return services;
    }

    public static IServiceCollection AddPasswordHasher(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        return services;
    }

    public static IServiceCollection AddJwtProvider(this IServiceCollection services)
    {
        services.AddScoped<IJwtProvider, JwtProvider>();

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

    public static IServiceCollection ConfigureMassTransit(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            x.AddRider(rider =>
            {
                // rider.AddConsumer<KafkaMessageConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    k.Host("localhost:9092");

                    // k.TopicEndpoint<KafkaMessage>("topic-name", "consumer-group-name", e =>
                    // {
                    //     e.ConfigureConsumer<KafkaMessageConsumer>(context);
                    // });
                });
            });
        });

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