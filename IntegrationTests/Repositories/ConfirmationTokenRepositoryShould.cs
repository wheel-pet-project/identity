using System.Data;
using Core.Domain.AccountAggregate;
using Core.Domain.ConfirmationTokenAggregate;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using Infrastructure.Adapters.Postgres.UnitOfWork;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Xunit;

namespace IntegrationTests.Repositories;

public class ConfirmationTokenRepositoryShould : IntegrationTestBase
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));

    [Fact]
    public async Task CanAddConfirmationToken()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('h', 60));
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        var accountRepository = unitOfWorkAndRepoBuilder.BuildAccountRepository();
        
        // Act
        await unitOfWork.BeginTransaction();
        await accountRepository.Add(_account);
        await repository.Add(confirmationToken);
        await unitOfWork.Commit();
    
        // Assert
        var confirmationTokenFromDb = await repository.Get(_account.Id);
        Assert.NotNull(confirmationTokenFromDb);
        Assert.Equivalent(confirmationToken, confirmationTokenFromDb);
    }

    [Fact]
    public async Task CanGetConfirmationToken()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('h', 60));
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        var accountRepository = unitOfWorkAndRepoBuilder.BuildAccountRepository();
        
        await unitOfWork.BeginTransaction();
        await accountRepository.Add(_account);
        await repository.Add(confirmationToken);
        await unitOfWork.Commit();
        
        // Act
        var confirmationTokenFromDb = await repository.Get(_account.Id);
    
        // Assert
        Assert.NotNull(confirmationTokenFromDb);
        Assert.Equivalent(confirmationToken, confirmationTokenFromDb);
    }

    [Fact]
    public async Task CanDeleteConfirmationToken()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('h', 60));
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWorkForAct, repositoryForAct) = unitOfWorkAndRepoBuilder.Build();
        var accountRepositoryForAct = unitOfWorkAndRepoBuilder.BuildAccountRepository();
        
        await unitOfWorkForAct.BeginTransaction();
        await accountRepositoryForAct.Add(_account);
        await repositoryForAct.Add(confirmationToken);
        await unitOfWorkForAct.Commit();
        
        unitOfWorkAndRepoBuilder.Reset();
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        
        // Act
        await unitOfWork.BeginTransaction();
        await repository.Delete(confirmationToken.AccountId);
        await unitOfWork.Commit();
    
        // Assert
        var confirmationTokenFromDb = await repository.Get(_account.Id);
        Assert.Null(confirmationTokenFromDb);
    }
    
    private class UnitOfWorkAndRepoBuilder
    {
        private string _connectionString = null!;
        private IDbConnection _connection = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        /// <summary>
        /// Создает экземпляры unitOfWork и сonfirmationTokenRepository из сессии созданной из подключения к БД
        /// </summary>
        /// <returns></returns>
        public (IUnitOfWork, IConfirmationTokenRepository) Build()
        {
            var unitOfWork = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var confirmationTokenRepository = new ConfirmationTokenRepository(_session,
                new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (unitOfWork, confirmationTokenRepository);
        }

        public void ConfigureConnection(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new NpgsqlConnection(_connectionString);
            _session = new DbSession(_connection);
        }
        
        public IAccountRepository BuildAccountRepository() => 
            new AccountRepository(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));

        /// <summary>
        /// Вызывает Dispose у подключения к БД и сессии и обновляет их
        /// </summary>
        public void Reset()
        {
            _session.Dispose();
            _connection = new NpgsqlConnection(_connectionString);
            _session = new DbSession(_connection);
        }
    }
}