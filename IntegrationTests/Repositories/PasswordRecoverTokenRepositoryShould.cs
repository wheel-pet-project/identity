using System.Data;
using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate;
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

public class PasswordRecoverTokenRepositoryShould : IntegrationTestBase
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));
    
    [Fact]
    public async Task CanAddRecoverToken()
    {
        // Arrange
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWorkForAct, _) = unitOfWorkAndRepoBuilder.Build();
        var accountRepository = unitOfWorkAndRepoBuilder.BuildAccountRepository();

        await unitOfWorkForAct.BeginTransaction();
        await accountRepository.Add(_account);
        await unitOfWorkForAct.Commit();
        
        unitOfWorkAndRepoBuilder.Reset();
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();

        // Act
        await unitOfWork.BeginTransaction();
        await repository.Add(passwordRecoverToken);
        await unitOfWork.Commit();
        
        // Assert
        var recoverTokenFromDb = await repository.Get(_account.Id);
        Assert.NotNull(recoverTokenFromDb);
        Assert.Equivalent(passwordRecoverToken, recoverTokenFromDb);
    }

    [Fact]
    public async Task CanGetRecoverToken()
    {
        // Arrange
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWorkForAct, _) = unitOfWorkAndRepoBuilder.Build();
        var accountRepository = unitOfWorkAndRepoBuilder.BuildAccountRepository();

        await unitOfWorkForAct.BeginTransaction();
        await accountRepository.Add(_account);
        await unitOfWorkForAct.Commit();
        
        unitOfWorkAndRepoBuilder.Reset();
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        await unitOfWork.BeginTransaction();
        await repository.Add(passwordRecoverToken);
        await unitOfWork.Commit();

        // Act
        var recoverTokenFromDb = await repository.Get(_account.Id);
        
        // Assert
        Assert.NotNull(recoverTokenFromDb);
        Assert.Equivalent(passwordRecoverToken, recoverTokenFromDb);
    }

    [Fact]
    public async Task CanDeleteRecoverToken()
    {
        // Arrange
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, repository) = unitOfWorkAndRepoBuilder.Build();

        // Act
        var recoverTokenFromDb = await repository.Get(_account.Id);
        
        // Assert
        Assert.Null(recoverTokenFromDb);
    }

    [Fact]
    public async Task CanUpdateAppliedStatus()
    {
        // Arrange
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));
        
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWorkForAct, repositoryForAct) = unitOfWorkAndRepoBuilder.Build();
        var accountRepository = unitOfWorkAndRepoBuilder.BuildAccountRepository();
        
        await unitOfWorkForAct.BeginTransaction();
        await accountRepository.Add(_account);
        await repositoryForAct.Add(passwordRecoverToken);
        await unitOfWorkForAct.Commit();
        
        unitOfWorkAndRepoBuilder.Reset();
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        
        // Act
        passwordRecoverToken.Apply();
        await unitOfWork.BeginTransaction();
        await repository.UpdateAppliedStatus(passwordRecoverToken);
        await unitOfWork.Commit();

        // Assert
        var recoverTokenFromDb = await repository.Get(_account.Id);
        Assert.NotNull(recoverTokenFromDb);
        Assert.Equivalent(passwordRecoverToken, recoverTokenFromDb);
    }
    
    private class UnitOfWorkAndRepoBuilder
    {
        private string _connectionString = null!;
        private IDbConnection _connection = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (IUnitOfWork, IPasswordRecoverTokenRepository) Build()
        {
            var unitOfWork = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var passwordRecoverTokenRepository =
                new PasswordRecoverTokenRepository(_session,
                    new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (unitOfWork, passwordRecoverTokenRepository);
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