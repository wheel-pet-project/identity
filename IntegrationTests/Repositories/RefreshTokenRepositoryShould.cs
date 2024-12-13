using System.Data;
using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Xunit;

namespace IntegrationTests.Repositories;

public class RefreshTokenRepositoryShould : RepositoryTestBase
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));
    
    [Fact]
    public async Task CanAddRefreshToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
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
        await repository.Add(refreshToken);
        await unitOfWork.Commit();

        // Assert
        var refreshTokenFromDb = await repository.GetNotRevokedToken(refreshToken.Id);
        Assert.NotNull(refreshTokenFromDb);
        Assert.Equivalent(refreshToken, refreshTokenFromDb);
    }

    [Fact]
    public async Task CanUpdateRevokeStatusRefreshToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWorkForAct, _) = unitOfWorkAndRepoBuilder.Build();
        var accountRepository = unitOfWorkAndRepoBuilder.BuildAccountRepository();

        await unitOfWorkForAct.BeginTransaction();
        await accountRepository.Add(_account);
        await unitOfWorkForAct.Commit();
        
        unitOfWorkAndRepoBuilder.Reset();
        var (unitOfWorkForAdd, repositoryForAdd) = unitOfWorkAndRepoBuilder.Build();
        await unitOfWorkForAdd.BeginTransaction();
        await repositoryForAdd.Add(refreshToken);
        await unitOfWorkForAdd.Commit();
        
        unitOfWorkAndRepoBuilder.Reset();
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        
        // Act
        refreshToken.Revoke();
        await unitOfWork.BeginTransaction();
        await repository.UpdateRevokeStatus(refreshToken);
        await unitOfWork.Commit();

        // Assert
        var refreshTokenFromDb = await repository.GetNotRevokedToken(refreshToken.Id);
        Assert.Null(refreshTokenFromDb);
    }

    [Fact]
    public async Task CanGetRefreshToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
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
        await repository.Add(refreshToken);
        await unitOfWork.Commit();
        
        // Act
        var refreshTokenFromDb = await repository.GetNotRevokedToken(refreshToken.Id);

        // Assert
        Assert.NotNull(refreshTokenFromDb);
        Assert.Equivalent(refreshToken, refreshTokenFromDb);
    }

    [Fact]
    public async Task CanAddTokenAndRevokeOldToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
        var newRefreshToken = RefreshToken.Create(_account);
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWorkForAct, repositoryForAct) = unitOfWorkAndRepoBuilder.Build();
        var accountRepository = unitOfWorkAndRepoBuilder.BuildAccountRepository();

        await unitOfWorkForAct.BeginTransaction();
        await accountRepository.Add(_account);
        await repositoryForAct.Add(refreshToken);
        await unitOfWorkForAct.Commit();
        
        unitOfWorkAndRepoBuilder.Reset();
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();

        // Act
        await unitOfWork.BeginTransaction();
        await repository.AddTokenAndRevokeOldToken(newRefreshToken, refreshToken);
        await unitOfWork.Commit();

        // Assert
        var oldRefreshTokenFromDb = await repository.GetNotRevokedToken(refreshToken.Id);
        var newRefreshTokenFromDb = await repository.GetNotRevokedToken(newRefreshToken.Id);
        Assert.Null(oldRefreshTokenFromDb);
        Assert.NotNull(newRefreshTokenFromDb);
        Assert.Equivalent(newRefreshToken, newRefreshTokenFromDb);
    }
    
    private class UnitOfWorkAndRepoBuilder
    {
        private string _connectionString = null!;
        private IDbConnection _connection = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (IUnitOfWork, IRefreshTokenRepository) Build()
        {
            var unitOfWork = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var refreshTokenRepository =
                new RefreshTokenRepository(_session,
                    new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (unitOfWork, refreshTokenRepository);
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