using Api.Adapters.Grpc.Mapper;
using Api.PipelineBehaviours;
using Core.Application.UseCases.CreateAccount;
using Core.Domain.Services.CreateAccountService;
using Core.Domain.Services.UpdateAccountPasswordService;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Kafka;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using From.IdentityKafkaEvents;
using Infrastructure.Adapters.Kafka;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Adapters.Postgres.Repositories;
using Infrastructure.Hasher;
using Infrastructure.JwtProvider;
using Infrastructure.Settings;
using MassTransit;
using MediatR;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;


namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPostgresDataSource(this IServiceCollection services)
    {
        services.AddTransient<NpgsqlDataSource>(_ =>
        {
            var sourceBuilder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = "Identity" + Environment.MachineName,
                    Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
                    Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT")!),
                    Database = Environment.GetEnvironmentVariable("POSTGRES_DB")!,
                    Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
                    Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")!,
                    BrowsableConnectionString = false,
                }
            };
            
            return sourceBuilder.Build();
        });
        
        return services;
    }

    public static IServiceCollection RegisterUnitOfWorkAndDbSession(this IServiceCollection services)
    {
        services.AddScoped<DbSession>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
    
    public static IServiceCollection RegisterOutbox(this IServiceCollection services)
    {
        services.AddTransient<IOutbox, Outbox>();
        
        return services;
    }

    public static IServiceCollection RegisterOutboxBackgroundJob(this IServiceCollection services)
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

    public static IServiceCollection RegisterMediatrAndPipelines(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountHandler).Assembly))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehaviour<,>));
        
        return services;
    }
    
    public static IServiceCollection RegisterMapper(this IServiceCollection services)
    {
        services.AddScoped<Mapper>();
        
        return services;
    }
    
    public static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddTransient<IPasswordRecoverTokenRepository, PasswordRecoverTokenRepository>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddTransient<IConfirmationTokenRepository, ConfirmationTokenRepository>();
    
        return services;
    }

    public static IServiceCollection RegisterDomainServices(this IServiceCollection services)
    {
        services.AddTransient<ICreateAccountService, CreateAccountService>();
        services.AddTransient<IUpdateAccountPasswordService, UpdateAccountPasswordService>();
        
        return services;
    }

    public static IServiceCollection RegisterPasswordHasher(this IServiceCollection services)
    {
        services.AddScoped<IHasher, Hasher>();

        return services;
    }

    public static IServiceCollection RegisterJwtProvider(this IServiceCollection services)
    {
        services.Configure<JwtOptions>(options =>
        {
            options.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "secret-key123456789101112";
            options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "carsharing-identity";
            options.AccessTokenExpirationMinutes =
                int.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES") ?? "15");
            options.RefreshTokenExpirationDays =
                int.Parse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS") ?? "21");
        });
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
    
    public static IServiceCollection RegisterSerilog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .WriteTo.MongoDBBson(Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")!,
                "logs",
                LogEventLevel.Verbose,
                50,
                TimeSpan.FromSeconds(10))
            .CreateLogger();
        services.AddSerilog();

        return services;
    }

    public static IServiceCollection RegisterMassTransit(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            x.AddRider(rider =>
            {
                rider.AddProducer<string, AccountCreated>("account-created-topic");
                rider.AddProducer<string, PasswordRecoverTokenCreated>("password-recover-token-created-topic");

                rider.UsingKafka((_, k) =>
                    k.Host(Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS")!.Split("__")));
            });
        });

        return services;
    }

    public static IServiceCollection RegisterMessageBus(this IServiceCollection services)
    {
        services.AddTransient<IMessageBus, KafkaProducer>();
        
        return services;
    }
    

    public static IServiceCollection RegisterTelemetry(this IServiceCollection services)
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
    
    public static IServiceCollection RegisterHealthCheckV1(this IServiceCollection services)
    {
        var getConnectionString = () =>
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder
            {
                ApplicationName = "Identity" + Environment.MachineName,
                Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
                Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT")!),
                Database = Environment.GetEnvironmentVariable("POSTGRES_DB")!,
                Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
                Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")!,
                BrowsableConnectionString = false,
            };
            
            return connectionBuilder.ConnectionString;
        };
        
        services.AddGrpcHealthChecks()
            .AddNpgSql(getConnectionString(), timeout: TimeSpan.FromSeconds(10))
            .AddKafka(cfg => 
                    cfg.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS")!.Split("__")[0], 
                timeout: TimeSpan.FromSeconds(10));
        
        return services;
    }

    public static IServiceCollection RegisterRetryPolicies(this IServiceCollection services)
    {
        services.AddTransient<PostgresRetryPolicy>();

        return services;
    }

    public static IServiceCollection RegisterTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        
        return services;
    }
}