using API.Extensions;
using Infrastructure.Settings;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        // builder.Logging.ClearProviders().AddConsole().AddDebug().AddOpenTelemetry(options =>
        // {
        //     options
        //         .SetResourceBuilder(ResourceBuilder
        //             .CreateDefault()
        //             .AddService("Identity"))
        //         .IncludeScopes = true;
        // });
        
        services.Configure<DbConnectionOptions>(builder.Configuration
            .GetSection("ConnectionStrings"));
        
        services.AddGrpc();
        
        ServiceCollectionExtensions.AddSqlMapperForEnums();
        
        // Extensions
        services
            .ConfigureSerilog()
            .ConfigureFluentEmail(builder.Configuration)
            .AddEmailProvider()
            .AddPasswordHasher()
            .AddTelemetry()
            .AddUseCases()
            .AddRepositories();
        
        var app = builder.Build();
        
        app.MapGrpcService<Services.Identity>();

        app.Run();
    }
}