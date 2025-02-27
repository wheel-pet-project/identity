using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Xunit;

namespace FunctionalTests;

public class FunctionalTestBase : IClassFixture<SubstituteWebApplicationFactory>, IAsyncDisposable
{
    private SubstituteWebApplicationFactory _factory;

    protected readonly HttpClient Client;

    public FunctionalTestBase(SubstituteWebApplicationFactory webApplicationFactory)
    {
        _factory = webApplicationFactory;
        Client = webApplicationFactory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.PostgreSqlContainer.DisposeAsync();
        await _factory.KafkaContainer.DisposeAsync();
    }
}