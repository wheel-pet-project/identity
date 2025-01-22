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
        await repository.Add(passwordRecoverToken);
        await uow.Commit();
        
        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var recoverTokenFromDb = await repoForAssert.Get(_account.Id);
        Assert.NotNull(recoverTokenFromDb);
        Assert.Equivalent(passwordRecoverToken, recoverTokenFromDb);
    }

    [Fact]
    public async Task CanGetRecoverToken()
    {
        // Arrange
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, _) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();

        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await uowForArrange.Commit();
        
        var (uow, repositoryForAdd) = uowAndRepoBuilder.Build();
        await uow.BeginTransaction();
        await repositoryForAdd.Add(passwordRecoverToken);
        await uow.Commit();
        
        var (_, repository) = uowAndRepoBuilder.Build();

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
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (_, repository) = uowAndRepoBuilder.Build();

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
        
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, repositoryForArrange) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();
        
        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await repositoryForArrange.Add(passwordRecoverToken);
        await uowForArrange.Commit();
        
        var (uow, repository) = uowAndRepoBuilder.Build();
        
        // Act
        passwordRecoverToken.Apply();
        await uow.BeginTransaction();
        await repository.UpdateAppliedStatus(passwordRecoverToken);
        await uow.Commit();

        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var recoverTokenFromDb = await repoForAssert.Get(_account.Id);
        Assert.NotNull(recoverTokenFromDb);
        Assert.Equivalent(passwordRecoverToken, recoverTokenFromDb);
    }
    
    private class UnitOfWorkAndRepoBuilder
    {
        private string _connectionString = null!;
        private NpgsqlDataSource _dataSource = null!;
        private DbSession? _session;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (IUnitOfWork, IPasswordRecoverTokenRepository) Build()
        {
            _session?.Dispose();
            _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            _session = new DbSession(_dataSource);
            
            var uow = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var passwordRecoverTokenRepository =
                new PasswordRecoverTokenRepository(_session,
                    new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (uow, passwordRecoverTokenRepository);
        }

        public void ConfigureConnection(string connectionString) => _connectionString = connectionString;

        public IAccountRepository BuildAccountRepository() => 
            new AccountRepository(_session!, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
    }
}