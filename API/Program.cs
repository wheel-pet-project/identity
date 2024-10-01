

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        
        services.AddGrpc();
        
        var app = builder.Build();
        
        app.MapGrpcService<Services.Identity>();

        app.Run();
    }
}