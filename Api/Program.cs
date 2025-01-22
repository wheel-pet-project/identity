using Api.Adapters.Grpc;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        
        services.AddGrpc(options => options.Interceptors.Add<ExceptionHandlerInterceptor>());
        
        // Extensions
        services
            .AddMediatrAndPipelines()
            .AddPostgresDataSource(builder.Configuration)
            .ConfigureMassTransit(builder.Configuration)
            .ConfigureSerilog(builder.Configuration)
            .AddHealthCheckV1(builder.Configuration)
            .AddJwtProvider(builder.Configuration)
            .AddOutboxBackgroundJob()
            .AddOutbox()
            .AddMessageBus()
            .AddUnitOfWorkAndDbSession()
            .AddRepositories()
            .AddDomainServices()
            .AddPasswordHasher()
            .AddRetryPolicies()
            .AddMapper()
            .AddTelemetry();
        
        
        var app = builder.Build();
        
        app.MapGrpcService<IdentityV1>();
        app.MapGrpcHealthChecksService();

        app.Run();
    }
}