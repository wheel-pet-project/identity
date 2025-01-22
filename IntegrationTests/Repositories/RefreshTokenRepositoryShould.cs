using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
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

public class RefreshTokenRepositoryShould : IntegrationTestBase
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));
    
    [Fact]
    public async Task CanAddRefreshToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, _) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();

        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await uowForArrange.Commit();
        
        var (uow, repository) = uowAndRepoBuilder.Build();
        
        // Act
        await uow.BeginTransaction();
        await repository.Add(refreshToken);
        await uow.Commit();

        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var refreshTokenFromDb = await repoForAssert.GetNotRevokedToken(refreshToken.Id);
        Assert.NotNull(refreshTokenFromDb);
        Assert.Equivalent(refreshToken, refreshTokenFromDb);
    }

    [Fact]
    public async Task CanUpdateRevokeStatusRefreshToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, _) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();

        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await uowForArrange.Commit();
        
        var (unitOfWorkForAdd, repositoryForAdd) = uowAndRepoBuilder.Build();
        await unitOfWorkForAdd.BeginTransaction();
        await repositoryForAdd.Add(refreshToken);
        await unitOfWorkForAdd.Commit();
        
        var (uow, repository) = uowAndRepoBuilder.Build();
        
        // Act
        refreshToken.Revoke();
        await uow.BeginTransaction();
        await repository.UpdateRevokeStatus(refreshToken);
        await uow.Commit();

        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var refreshTokenFromDb = await repoForAssert.GetNotRevokedToken(refreshToken.Id);
        Assert.Null(refreshTokenFromDb);
    }

    [Fact]
    public async Task CanGetRefreshToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, _) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();

        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await uowForArrange.Commit();
        
        var (uowForAdd, repositoryForAdd) = uowAndRepoBuilder.Build();
        
        await uowForAdd.BeginTransaction();
        await repositoryForAdd.Add(refreshToken);
        await uowForAdd.Commit();
        
        var (_, repository) = uowAndRepoBuilder.Build();
        
        // Act
        var refreshTokenFromDb = await repository.GetNotRevokedToken(refreshToken.Id);

        // Assert
        Assert.NotNull(refreshTokenFromDb);
        Assert.Equivalent(refreshToken, refreshTokenFromDb);
    }
    
    [Fact]
    public async Task CanGetNotRevokedRefreshTokens()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, _) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();

        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await uowForArrange.Commit();
        
        var (uowForAdd, repositoryForAdd) = uowAndRepoBuilder.Build();
        
        await uowForAdd.BeginTransaction();
        await repositoryForAdd.Add(refreshToken);
        await uowForAdd.Commit();
        
        var (_, repository) = uowAndRepoBuilder.Build();
        
        // Act
        var refreshTokensFromDb = await repository.GetNotRevokedTokensByAccountId(_account.Id);

        // Assert
        Assert.NotEmpty(refreshTokensFromDb);
        Assert.Equivalent(refreshToken, refreshTokensFromDb[0]);
    }
    

    [Fact]
    public async Task CanAddTokenAndRevokeOldToken()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);
        var newRefreshToken = RefreshToken.Create(_account);
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, repositoryForAct) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();

        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await repositoryForAct.Add(refreshToken);
        await uowForArrange.Commit();
        
        var (uow, repository) = uowAndRepoBuilder.Build();

        // Act
        await uow.BeginTransaction();
        await repository.AddTokenAndRevokeOldToken(newRefreshToken, refreshToken);
        await uow.Commit();

        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var oldRefreshTokenFromDb = await repoForAssert.GetNotRevokedToken(refreshToken.Id);
        var newRefreshTokenFromDb = await repoForAssert.GetNotRevokedToken(newRefreshToken.Id);
        Assert.Null(oldRefreshTokenFromDb);
        Assert.NotNull(newRefreshTokenFromDb);
        Assert.Equivalent(newRefreshToken, newRefreshTokenFromDb);
    }
    
    private class UnitOfWorkAndRepoBuilder
    {
        private string _connectionString = null!;
        private NpgsqlDataSource _dataSource = null!;
        private DbSession? _session;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (IUnitOfWork, IRefreshTokenRepository) Build()
        {
            _session?.Dispose();
            _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            _session = new DbSession(_dataSource);
            
            var uow = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var refreshTokenRepository =
                new RefreshTokenRepository(_session,
                    new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (uow, refreshTokenRepository);
        }

        public void ConfigureConnection(string connectionString) => _connectionString = connectionString;

        public IAccountRepository BuildAccountRepository() => 
            new AccountRepository(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));

        /// <summary>
        /// Вызывает Dispose у подключения к БД и сессии и обновляет их
        /// </summary>
        public void Reset()
        {
            _session.Dispose();
            _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            _session = new DbSession(_dataSource);
        }
    }
}