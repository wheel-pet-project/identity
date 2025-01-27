using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel;
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

public class AccountRepositoryShould : IntegrationTestBase
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));

    [Fact]
    public async Task CanAddAccount()
    {
        // Arrange
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uow, repository) = uowAndRepoBuilder.Build();
        
        // Act
        await uow.BeginTransaction();
        await repository.Add(_account); 
        await uow.Commit();
        
        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var accountFromDb = await repoForAssert.GetById(_account.Id);
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(_account, accountFromDb);
    }

    [Fact]
    public async Task CanGetByIdAccount()
    {
        // Arrange
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uow, repositoryForArrange) = uowAndRepoBuilder.Build();
        await uow.BeginTransaction();
        await repositoryForArrange.Add(_account);
        await uow.Commit();
        
        var (_, repository) = uowAndRepoBuilder.Build();
        
        // Act
        var accountFromDb = await repository.GetById(_account.Id);
        
        // Assert
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(_account, accountFromDb);
    }
    
    [Fact]
    public async Task CanGetByEmailAccount()
    {
        // Arrange
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uow, repositoryForArrange) = uowAndRepoBuilder.Build();
        await uow.BeginTransaction();
        await repositoryForArrange.Add(_account);
        await uow.Commit();
        
        var (_, repository) = uowAndRepoBuilder.Build();
        
        // Act
        var accountFromDb = await repository.GetByEmail(_account.Email);
        
        // Assert
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(_account, accountFromDb);
    }
    
    [Fact]
    public async Task CanUpdateStatusAccount()
    {
        // Arrange
        var accountForUpdateStatusTest = Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uowForArrange, repositoryForArrange) = uowAndRepoBuilder.Build();
        await uowForArrange.BeginTransaction();
        await repositoryForArrange.Add(accountForUpdateStatusTest);
        await uowForArrange.Commit();
        
        var (uow, repository) = uowAndRepoBuilder.Build();
        
        // Act
        accountForUpdateStatusTest.SetStatus(Status.PendingApproval);
        await uow.BeginTransaction();
        await repository.UpdateStatus(accountForUpdateStatusTest);
        await uow.Commit();
        
        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var accountFromDb = await repoForAssert.GetById(accountForUpdateStatusTest.Id);
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(accountForUpdateStatusTest, accountFromDb);
    }
    
    [Fact]
    public async Task CanUpdatePasswordHashAccount()
    {
        // Arrange
        var accountForUpdatePassHash =
            Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));
        var uowAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        uowAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (uow, repositoryForArrange) = uowAndRepoBuilder.Build();
        await uow.BeginTransaction();
        await repositoryForArrange.Add(accountForUpdatePassHash);
        await uow.Commit();
        
        var (uowForAct, repository) = uowAndRepoBuilder.Build();
        
        // Act
        accountForUpdatePassHash.SetPasswordHash(new string('-', 60));
        await uowForAct.BeginTransaction();
        await repository.UpdatePasswordHash(accountForUpdatePassHash);
        await uowForAct.Commit();
        
        // Assert
        var (_, repoForAssert) = uowAndRepoBuilder.Build();
        var accountFromDb = await repoForAssert.GetById(accountForUpdatePassHash.Id);
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(accountForUpdatePassHash.PasswordHash, accountFromDb.PasswordHash);
    }

    private class UnitOfWorkAndRepoBuilder
    {
        private NpgsqlDataSource _dataSource = null!;
        private DbSession? _session;
        private string _connectionString = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (IUnitOfWork, IAccountRepository) Build()
        {
            _session?.Dispose();
            _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
            _session = new DbSession(_dataSource);
            
            var uow = new UnitOfWork(_session!, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var accountRepository =
                new AccountRepository(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (uow, accountRepository);
        }

        public void ConfigureConnection(string connectionString) => _connectionString = connectionString;
    }
}