using Api.Adapters.Grpc;
using Api.Interceptors;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionHandlerInterceptor>();
            options.Interceptors.Add<TracingInterceptor>();
            options.Interceptors.Add<LoggingInterceptor>();
        });

        // Extensions
        services
            .RegisterTelemetry()
            .RegisterMediatrAndPipelines()
            .RegisterPostgresDataSource()
            .RegisterMassTransit()
            .RegisterSerilog()
            .RegisterHealthCheckV1()
            .RegisterJwtProvider()
            .RegisterOutboxBackgroundJob()
            .RegisterOutbox()
            .RegisterMessageBus()
            .RegisterUnitOfWorkAndDbSession()
            .RegisterRepositories()
            .RegisterDomainServices()
            .RegisterPasswordHasher()
            .RegisterRetryPolicies()
            .RegisterMapper()
            .RegisterTimeProvider();


        var app = builder.Build();

        app.MapGrpcService<IdentityV1>();
        app.MapGrpcHealthChecksService();

        app.Run();
    }
}