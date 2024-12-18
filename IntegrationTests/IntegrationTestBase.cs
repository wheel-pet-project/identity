using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests;

public class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("identity")
        .WithUsername("postgres")
        .WithPassword("password")
        .WithCleanUp(true)
        .Build();
    
    public async Task InitializeAsync()
    {
        await PostgreSqlContainer.StartAsync();

        var sql = await File.ReadAllTextAsync("db.sql");
        
        var connection = new NpgsqlConnection(PostgreSqlContainer.GetConnectionString());

        await connection.OpenAsync();
        var command = new CommandDefinition(sql);
        await connection.ExecuteAsync(command);
    }

    public async Task DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
    }
}