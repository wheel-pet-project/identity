using Core.Domain.AccountAggregate;
using Core.Domain.ConfirmationTokenAggregate;
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

public class ConfirmationTokenRepositoryShould : IntegrationTestBase
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60), Guid.NewGuid());

    [Fact]
    public async Task CanAddConfirmationToken()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('h', 60));
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uow, repository) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();
        
        // Act
        await uow.BeginTransaction();
        await accountRepository.Add(_account);
        await repository.Add(confirmationToken);
        await uow.Commit();
    
        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var confirmationTokenFromDb = await repoForAssert.Get(_account.Id);
        Assert.NotNull(confirmationTokenFromDb);
        Assert.Equivalent(confirmationToken, confirmationTokenFromDb);
    }

    [Fact]
    public async Task CanGetConfirmationToken()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('h', 60));
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, repositoryForArrange) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();
        
        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await repositoryForArrange.Add(confirmationToken);
        await uowForArrange.Commit();
        
        var (_, repository) = uowAndRepoBuilder.Build();
        
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
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, repositoryForArrange) = uowAndRepoBuilder.Build();
        var accountRepository = uowAndRepoBuilder.BuildAccountRepository();
        
        await uowForArrange.BeginTransaction();
        await accountRepository.Add(_account);
        await repositoryForArrange.Add(confirmationToken);
        await uowForArrange.Commit();
        
        var (uow, repository) = uowAndRepoBuilder.Build();
        
        // Act
        await uow.BeginTransaction();
        await repository.Delete(confirmationToken.AccountId);
        await uow.Commit();
    
        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var confirmationTokenFromDb = await repoForAssert.Get(_account.Id);
        Assert.Null(confirmationTokenFromDb);
    }
    
    private class UnitOfWorkAndRepoBuilder
    {
        private string _connectionString = null!;
        private NpgsqlDataSource _dataSource = null!;
        private DbSession? _session;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();
        
        public (IUnitOfWork, IConfirmationTokenRepository) Build()
        {
            _session?.Dispose();
            _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            _session = new DbSession(_dataSource);
            
            var uow = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var confirmationTokenRepository = new ConfirmationTokenRepository(_session,
                new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (uow, confirmationTokenRepository);
        }

        public void ConfigureConnection(string connectionString) => _connectionString = connectionString;

        public IAccountRepository BuildAccountRepository() => 
            new AccountRepository(_session!, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
    }
}