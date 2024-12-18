using System.Data;
using Core.Ports.Postgres;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Adapters.Postgres.UnitOfWork;
using Infrastructure.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Xunit;

namespace IntegrationTests.Outbox;

public class OutboxBackgroundJobShould
{
    
    private class OutboxBackgroundJobBuilder
    {
        private DbSession _session = null!;
        private readonly Mock<IMediator> _mediatorMock = new();
        
        OutboxBackgroundJob Build() => new(_session, _mediatorMock.Object);

        public void ConfigureConnection(string connectionString)
        {
            _session = new DbSession(new NpgsqlConnection(connectionString));
        }
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
    }
}