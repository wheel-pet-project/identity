using Api.Adapters.Grpc;
using Api.Interceptors;
using Api.Registrars;
using Api.Extensions;
using Infrastructure.Settings;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
        SqlMapperRegister.Register();
        
        services.AddGrpc(options => 
            options.Interceptors.Add<ExceptionHandlerInterceptor>());
        
        // Extensions
        services
            .ConfigureSerilog(builder.Configuration)
            .AddDbConnection(builder.Configuration)
            .AddHealthCheckV1(builder.Configuration)
            .AddRetryPolicies()
            .AddPasswordHasher()
            .AddJwtProvider()
            .AddTelemetry()
            .AddUseCases()
            .AddRepositories();
        
        var app = builder.Build();
        
        app.MapGrpcService<IdentityV1>();
        app.MapGrpcHealthChecksService();

        app.Run();
    }
}