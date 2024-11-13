using API.Extensions;
using API.Interceptors;
using API.Registrars;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace API;

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
            .AddRetryPolicy()
            .AddPasswordHasher()
            .AddJwtProvider()
            .AddTelemetry()
            .AddUseCases()
            .AddRepositories();
        
        var app = builder.Build();
        
        app.MapGrpcService<Services.IdentityV1>();
        app.MapGrpcHealthChecksService();
        app.MapHealthChecks("health");

        app.Run();
    }
}