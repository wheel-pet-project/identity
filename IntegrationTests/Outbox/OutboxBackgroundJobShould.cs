using System.Data;
using Core.Domain.ConfirmationTokenAggregate;
using Core.Ports.Postgres;
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
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests.Outbox;

public class OutboxBackgroundJobShould : IntegrationTestBase
{
    [Fact]
    public async Task CanReadAndMediatorPublishNotificationTwoTimes()
    {
        // Arrange
        var aggregate = ConfirmationToken.Create(accountId: Guid.NewGuid(), new string('*', 60));
        aggregate.AddCreatedDomainEvent(Guid.NewGuid());
        aggregate.AddCreatedDomainEvent(Guid.NewGuid());
        
        var unitOfWorkAndOutboxBuilder = new UnitOfWorkAndOutboxBuilder();
        unitOfWorkAndOutboxBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, outbox) = unitOfWorkAndOutboxBuilder.Build();
        
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
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (IUnitOfWork, IOutbox) Build()
        {
            var unitOfWork = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var outbox = new Infrastructure.Adapters.Postgres.Outbox.Outbox(_session);
            return (unitOfWork, outbox);
        }

        public void ConfigureConnection(string connectionString)
        {
            _session = new DbSession(new NpgsqlConnection(connectionString));
        }
    }
}