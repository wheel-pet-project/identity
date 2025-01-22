using System.Collections.Immutable;
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
    private const string Query = @"SELECT event_id AS EventId, type AS Type, content AS Content, 
       occurred_on_utc AS OccurredOnUtc, processed_on_utc AS ProcessedOnUtc
FROM outbox";
    
    [Fact]
    public async Task CanReadAndMediatorPublishNotificationTwoTimes()
    {
        // Arrange
        var aggregate = ConfirmationToken.Create(accountId: Guid.NewGuid(), new string('*', 60));
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        
        var uowAndOutboxBuilder = new UnitOfWorkAndOutboxBuilder();
        uowAndOutboxBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, uow, outbox) = uowAndOutboxBuilder.Build();
        
        await uow.BeginTransaction();
        await outbox.PublishDomainEvents(aggregate);
        await uow.Commit();
        
        var outboxJobBuilder = new OutboxBackgroundJobBuilder();
        outboxJobBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var outboxJob = outboxJobBuilder.Build();

        // Act
        await outboxJob.Execute(new Mock<IJobExecutionContext>().Object);

        // Assert
        Assert.True(outboxJobBuilder.VerifyMediatorPublishMethod(2));
    }

    [Fact]
    public async Task CanMarkEventsAsProcessed()
    {
        // Arrange
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        var aggregate = ConfirmationToken.Create(accountId: Guid.NewGuid(), new string('*', 60));
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        aggregate.AddCreatedDomainEvent(Guid.NewGuid(), "email@domain.com");
        
        var uowAndOutboxBuilder = new UnitOfWorkAndOutboxBuilder();
        uowAndOutboxBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, uow, outbox) = uowAndOutboxBuilder.Build();
        
        await uow.BeginTransaction();
        await outbox.PublishDomainEvents(aggregate);
        await uow.Commit();
        
        var outboxJobBuilder = new OutboxBackgroundJobBuilder();
        outboxJobBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var outboxJob = outboxJobBuilder.Build();

        // Act
        await outboxJob.Execute(new Mock<IJobExecutionContext>().Object);
        
        // Arrange
        uowAndOutboxBuilder.Reset();
        var (session, _, _) = uowAndOutboxBuilder.Build();
        var outboxEvents = await session.Connection.QueryAsync<OutboxEvent>(Query);
        var eventModels = outboxEvents.AsList();
        
        Assert.True(eventModels.All(x => x.ProcessedOnUtc != null)); ;
    }
    
    private class OutboxBackgroundJobBuilder
    {
        private NpgsqlDataSource _dataSource = null!;
        private readonly Mock<IMediator> _mediatorMock = new();
        
        public OutboxBackgroundJob Build() => new(_dataSource, _mediatorMock.Object);

        public void ConfigureConnection(string connectionString) =>
            _dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();

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
        private NpgsqlDataSource _dataSource = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (DbSession, IUnitOfWork, IOutbox) Build()
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