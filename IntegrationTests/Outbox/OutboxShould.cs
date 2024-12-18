using System.Data;
using System.Net.Http.Json;
using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.SharedKernel;
using Core.Ports.Postgres;
using Dapper;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Adapters.Postgres.UnitOfWork;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Xunit;

namespace IntegrationTests.Outbox;

public class OutboxShould : IntegrationTestBase
{
    private const string query = @"SELECT event_id AS EventId, type AS Type, content AS Content, 
       occurred_on_utc AS OccurredOnUtc, processed_on_utc AS ProcessedOnUtc
FROM outbox";
    
    [Fact]
    public async Task CanAddOutboxEvents()
    {
        // Arrange
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        
        var aggregate = ConfirmationToken.Create(accountId: Guid.NewGuid(), new string('*', 60));
        aggregate.AddCreatedDomainEvent(Guid.NewGuid());
        var expectedEventId = aggregate.DomainEvents.First().EventId;
        
        var unitOfWorkAndOutboxBuilder = new UnitOfWorkAndOutboxBuilder();
        unitOfWorkAndOutboxBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, unitOfWork, outbox) = unitOfWorkAndOutboxBuilder.Build();

        // Act
        await unitOfWork.BeginTransaction();
        await outbox.PublishDomainEvents(aggregate);
        await unitOfWork.Commit();
        
        // Assert
        unitOfWorkAndOutboxBuilder.Reset();
        var (session, _, _) = unitOfWorkAndOutboxBuilder.Build();
        var outboxEvents = await session.Connection.QueryAsync<OutboxEventModel>(query);
        var eventModels = outboxEvents.ToList();
        
        Assert.True(eventModels.Any());
        var @event = JsonConvert.DeserializeObject<DomainEvent>(eventModels.FirstOrDefault()!.Content, jsonSettings);
        Assert.Equivalent(expectedEventId, @event!.EventId);
    }

    private class UnitOfWorkAndOutboxBuilder
    {
        private string _connectionString = null!;
        private IDbConnection _connection = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (DbSession ,IUnitOfWork, IOutbox) Build()
        {
            var unitOfWork = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var outbox = new Infrastructure.Adapters.Postgres.Outbox.Outbox(_session);
            return (_session, unitOfWork, outbox);
        }

        public void ConfigureConnection(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new NpgsqlConnection(_connectionString);
            _session = new DbSession(_connection);
        }

        public void Reset()
        {
            _session.Dispose();
            _connection = new NpgsqlConnection(_connectionString);
            _session = new DbSession(_connection);
        }
    }
}