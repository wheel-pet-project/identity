using System.Data;
using Api.PipelineBehaviours;
using Api.Settings;
using Core.Application.UseCases.CreateAccount;
using Core.Domain.Services;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Kafka;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentValidation;
using Infrastructure.Adapters.Kafka;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Adapters.Postgres.Repositories;
using Infrastructure.Adapters.Postgres.UnitOfWork;
using Infrastructure.Hasher;
using Infrastructure.JwtProvider;
using Infrastructure.Settings;
using MassTransit;
using MediatR;
using NotificationKafkaEvents;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgres(this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection("DbConnectionSettings").Get<DbConnectionSettings>();
        services.AddScoped<IDbConnection>(_ =>
        {
            var sourceBuilder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = "Identity",
                    Host = dbSettings!.Host,
                    Port = dbSettings.Port,
                    Database = dbSettings.Database,
                    Username = dbSettings.Username,
                    Password = dbSettings.Password
                }
            };
            var dataSource = sourceBuilder.Build();
            
            return dataSource.CreateConnection();
        });
        services.AddScoped<DbSession>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        
        return services;
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }

    public static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddTransient<IOutbox, Outbox>();
        
        return services;
    }

    public static IServiceCollection AddOutboxBackgroundJob(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            var jobKey = new JobKey(nameof(OutboxBackgroundJob));
            configure
                .AddJob<OutboxBackgroundJob>(j => j.WithIdentity(jobKey))
                .AddTrigger(trigger => trigger.ForJob(jobKey)
                    .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(3).RepeatForever()));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        
        return services;
    }

    public static IServiceCollection AddMediatrAndHandlers(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountHandler).Assembly))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));
        
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(CreateAccountRequestValidator).Assembly);
        
        return services;
    }

    public static IServiceCollection AddMapper(this IServiceCollection services)
    {
        services.AddScoped<Mapper.Mapper>();
        
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddTransient<IPasswordRecoverTokenRepository, PasswordRecoverTokenRepository>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddTransient<IConfirmationTokenRepository, ConfirmationTokenRepository>();

        return services;
    }

    public static IServiceCollection AddDomainService(this IServiceCollection services)
    {
        services.AddTransient<ICreateAccountService, CreateAccountService>();
        
        return services;
    }

    public static IServiceCollection AddPasswordHasher(this IServiceCollection services)
    {
        services.AddScoped<IHasher, Hasher>();

        return services;
    }

    public static IServiceCollection AddJwtProvider(this IServiceCollection services)
    {
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
    
    public static IServiceCollection ConfigureSerilog(this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection("MongoSettings").Get<MongoSettings>();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .WriteTo.MongoDBBson(settings!.ConnectionString,
                "logs",
                LogEventLevel.Verbose,
                50,
                TimeSpan.FromSeconds(10))
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
                rider.AddProducer<string, ConfirmationTokenCreated>("send-confirmation-email-topic");
                rider.AddProducer<string, PasswordRecoverTokenCreated>("send-recover-password-email-topic");

                rider.UsingKafka((_, k) => { k.Host("localhost:9092"); });
            });
        });

        return services;
    }

    public static IServiceCollection AddMessageBus(this IServiceCollection services)
    {
        services.AddTransient<IMessageBus, KafkaProducer>();
        
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
                    .AddSource("MassTransit") 
                    .AddJaegerExporter();
            });

        return services;
    }
    
    public static IServiceCollection AddHealthCheckV1(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection("DbConnectionSettings").Get<DbConnectionSettings>();

        var getConnectionString = () => 
        {
            var sourceBuilder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = "Identity",
                    Host = dbSettings!.Host,
                    Port = dbSettings.Port,
                    Database = dbSettings.Database,
                    Username = dbSettings.Username,
                    Password = dbSettings.Password
                }
            };
            return sourceBuilder.ConnectionStringBuilder.ConnectionString;
        };
        
        // todo: change strong typing connection string to kafka
        services.AddGrpcHealthChecks()
            .AddNpgSql(getConnectionString(), timeout: TimeSpan.FromSeconds(10))
            .AddKafka(cfg => cfg.BootstrapServers = "localhost:9092", timeout: TimeSpan.FromSeconds(10));
        
        return services;
    }

    public static IServiceCollection AddRetryPolicies(this IServiceCollection services)
    {
        services.AddTransient<PostgresRetryPolicy>();

        return services;
    }
}