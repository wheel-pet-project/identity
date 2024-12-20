using System.Data;
using Core.Domain.ConfirmationTokenAggregate;
using Core.Ports.Postgres;
using Dapper;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Adapters.Postgres.UnitOfWork;
using Infrastructure.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Quartz;
using Xunit;

namespace IntegrationTests.Outbox;

public class OutboxBackgroundJobShould : IntegrationTestBase
{
    private const string query = @"SELECT event_id AS EventId, type AS Type, content AS Content, 
       occurred_on_utc AS OccurredOnUtc, processed_on_utc AS ProcessedOnUtc
FROM outbox";
    
    [Fact]
    public async Task CanReadAndMediatorPublishNotificationTwoTimes()
    {
        // Arrange
        var aggregate = ConfirmationToken.Create(accountId: Guid.NewGuid(), new string('*', 60));
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        
        var unitOfWorkAndOutboxBuilder = new UnitOfWorkAndOutboxBuilder();
        unitOfWorkAndOutboxBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, unitOfWork, outbox) = unitOfWorkAndOutboxBuilder.Build();
        
        await unitOfWork.BeginTransaction();
        await outbox.PublishDomainEvents(aggregate);
        await unitOfWork.Commit();
        
        var outboxBackgroundJobBuilder = new OutboxBackgroundJobBuilder();
        outboxBackgroundJobBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var outboxBackJob = outboxBackgroundJobBuilder.Build();

        // Act
        await outboxBackJob.Execute(new Mock<IJobExecutionContext>().Object);

        // Assert
        Assert.True(outboxBackgroundJobBuilder.VerifyMediatorPublishMethod(2));
    }

    [Fact]
    public async Task CanMarkEventsAsProcessed()
    {
        // Arrange
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        var aggregate = ConfirmationToken.Create(accountId: Guid.NewGuid(), new string('*', 60));
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        
        var unitOfWorkAndOutboxBuilder = new UnitOfWorkAndOutboxBuilder();
        unitOfWorkAndOutboxBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, unitOfWork, outbox) = unitOfWorkAndOutboxBuilder.Build();
        
        await unitOfWork.BeginTransaction();
        await outbox.PublishDomainEvents(aggregate);
        await unitOfWork.Commit();
        
        var outboxBackgroundJobBuilder = new OutboxBackgroundJobBuilder();
        outboxBackgroundJobBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var outboxBackJob = outboxBackgroundJobBuilder.Build();

        // Act
        await outboxBackJob.Execute(new Mock<IJobExecutionContext>().Object);
        
        // Arrange
        unitOfWorkAndOutboxBuilder.Reset();
        var (session, _, _) = unitOfWorkAndOutboxBuilder.Build();
        var outboxEvents = await session.Connection.QueryAsync<OutboxEventModel>(query);
        var eventModels = outboxEvents.ToList();
        
        Assert.True(eventModels.All(x => x.ProcessedOnUtc != default)); ;
    }
    
    private class OutboxBackgroundJobBuilder
    {
        private DbSession _session = null!;
        private readonly Mock<IMediator> _mediatorMock = new();
        
        public OutboxBackgroundJob Build() => new(_session, _mediatorMock.Object);

        public void ConfigureConnection(string connectionString) =>
            _session = new DbSession(new NpgsqlConnection(connectionString));

        public bool VerifyMediatorPublishMethod(int times)
        {
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Exactly(times));
            return true;
        }
    }
    
    private class UnitOfWorkAndOutboxBuilder
    {
        private string _connectionString = null!;
        private IDbConnection _connection = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (DbSession, IUnitOfWork, IOutbox) Build()
        {
            var unitOfWork = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var outbox = new Infrastructure.Adapters.Postgres.Outbox.Outbox(_session);
            return (_session, unitOfWork, outbox);
        }

        public void ConfigureConnection(string connectionString)
        {
            _connectionString = connectionString;
            _session = new DbSession(new NpgsqlConnection(_connectionString));
        }

        public void Reset()
        {
            _session.Dispose();
            _connection = new NpgsqlConnection(_connectionString);
            _session = new DbSession(_connection);
        }
    }
}