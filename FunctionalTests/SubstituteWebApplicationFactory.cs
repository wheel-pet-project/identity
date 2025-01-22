using System.Collections.ObjectModel;
using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;

namespace FunctionalTests;

public class SubstituteWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("identity")
        .WithUsername("postgres")
        .WithPassword("password")
        .WithCleanUp(true)
        .Build();

    public readonly KafkaContainer KafkaContainer = new KafkaBuilder()
        .WithImage("bitnami/kafka:latest")
        .WithEnvironment("AUTO_CREATE_TOPICS", "true")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionBuilder = new NpgsqlConnectionStringBuilder(PostgreSqlContainer.GetConnectionString());
        
        var testAppSettings = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { "DbConnectionSettings:Host", connectionBuilder.Host! },
            { "DbConnectionSettings:Port", connectionBuilder.Port.ToString() },
            { "DbConnectionSettings:Database", connectionBuilder.Database! },
            { "DbConnectionSettings:Username", connectionBuilder.Username! },
            { "DbConnectionSettings:Password", connectionBuilder.Password! },
            
            { "JwtOptions:SecretKey", "obz)1M@TKw{UJrI–xvf:})<}3<VMdk}#R<KD;GG6tp$?CnM^UхI[c^bLG$V}&VEa" },
            { "JwtOptions:Issuer", "carsharing-identity-service" },
            { "JwtOptions:AccessTokenExpirationMinutes", "15" },
            { "JwtOptions:RefreshTokenExpirationDays", "21" },
            
            { "MongoSettings:ConnectionString", "mongodb://carsharing:12s56daw4gby@localhost:27017/identity?authSource=admin" },
            
            { "KafkaSettings:BootstrapServers", KafkaContainer.GetBootstrapAddress() },
        });
        
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(testAppSettings!).Build();
        
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
        {
            services
                .AddMediatrAndPipelines()
                .AddPostgresDataSource(configuration)  // 
                .AddJwtProvider(configuration)         // configuration replaced
                .ConfigureMassTransit(configuration)   // 
                .AddOutboxBackgroundJob()
                .AddOutbox()
                .AddMessageBus()
                .AddUnitOfWorkAndDbSession()
                .AddRepositories()
                .AddDomainServices()
                .AddPasswordHasher()
                .AddRetryPolicies()
                .AddMapper();
        });
        
        base.ConfigureWebHost(builder); 
        // todo: возможно лучше перенести настройку в этот метод, чтобы сервисы конфигурировались в Program.cs, а тут только заменялись необходимые 
    }
}