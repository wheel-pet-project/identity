using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.SharedKernel;
using Core.Ports.Postgres;
using Dapper;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Settings;
using JsonNet.ContractResolvers;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Xunit;

namespace IntegrationTests.Outbox;

public class OutboxShould : IntegrationTestBase
{
    private const string Query = @"SELECT event_id AS EventId, type AS Type, content AS Content, 
       occurred_on_utc AS OccurredOnUtc, processed_on_utc AS ProcessedOnUtc
FROM outbox";
    
    [Fact]
    public async Task CanAddOutboxEvents()
    {
        // Arrange
        var jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver()
        };
        
        var aggregate = ConfirmationToken.Create(accountId: Guid.NewGuid(), new string('*', 60));
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        var expectedEventId = aggregate.DomainEvents.First().EventId;
        
        var uowAndOutboxBuilder = new UnitOfWorkAndOutboxBuilder();
        uowAndOutboxBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, uow, outbox) = uowAndOutboxBuilder.Build();

        // Act
        await uow.BeginTransaction();
        await outbox.PublishDomainEvents(aggregate);
        await uow.Commit();
        
        // Assert
        uowAndOutboxBuilder.Reset();
        var (session, _, _) = uowAndOutboxBuilder.Build();
        var outboxEvents = await session.Connection.QueryAsync<OutboxEvent>(Query);
        var eventModels = outboxEvents.AsList();
        
        Assert.True(eventModels.Any());
        Assert.Equal(expectedEventId, eventModels.First().EventId);
        var @event = JsonConvert.DeserializeObject<DomainEvent>(eventModels.FirstOrDefault()!.Content, jsonSettings);
        Assert.Equivalent(expectedEventId, @event!.EventId);
    }

    private class UnitOfWorkAndOutboxBuilder
    {
        private string _connectionString = null!;
        private NpgsqlDataSource _dataSource = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (DbSession ,IUnitOfWork, IOutbox) Build()
        {
            var uow = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var outbox = new Infrastructure.Adapters.Postgres.Outbox.Outbox(_session);
            return (_session, uow, outbox);
        }

        public void ConfigureConnection(string connectionString)
        {
            _connectionString = connectionString;
            _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            _session = new DbSession(_dataSource);
        }

        public void Reset()
        {
            _session.Dispose();
            _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            _session = new DbSession(_dataSource);
        }
    }
}