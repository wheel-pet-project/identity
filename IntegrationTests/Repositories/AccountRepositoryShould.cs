using System.Data;
using Core.Domain.AccountAggregate;
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

public class AccountRepositoryShould : IntegrationTestBase
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));

    [Fact]
    public async Task CanAddAccount()
    {
        // Arrange
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        
        // Act
        await unitOfWork.BeginTransaction();
        await repository.Add(_account); 
        await unitOfWork.Commit();
        
        // Assert
        var accountFromDb = await repository.GetById(_account.Id);
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(_account, accountFromDb);
    }

    [Fact]
    public async Task CanGetByIdAccount()
    {
        // Arrange
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        await unitOfWork.BeginTransaction();
        await repository.Add(_account);
        await unitOfWork.Commit();
        
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
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        await unitOfWork.BeginTransaction();
        await repository.Add(_account);
        await unitOfWork.Commit();
        
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
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        await unitOfWork.BeginTransaction();
        await repository.Add(accountForUpdateStatusTest);
        await unitOfWork.Commit();
        
        // Act
        accountForUpdateStatusTest.SetStatus(Status.PendingApproval);
        await unitOfWork.BeginTransaction();
        await repository.UpdateStatus(accountForUpdateStatusTest);
        await unitOfWork.Commit();
        
        // Assert
        var accountFromDb = await repository.GetById(accountForUpdateStatusTest.Id);
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(accountForUpdateStatusTest, accountFromDb);
    }
    
    [Fact]
    public async Task CanUpdatePasswordHashAccount()
    {
        // Arrange
        var accountForUpdatePassHash =
            Account.Create(Role.Customer, "email@email.com", "+79007006050", new string('*', 60));
        var unitOfWorkAndRepoBuilder = new UnitOfWorkAndRepoBuilder();
        unitOfWorkAndRepoBuilder.ConfigureConnection(PostgreSqlContainer.GetConnectionString());
        var (unitOfWork, repository) = unitOfWorkAndRepoBuilder.Build();
        await unitOfWork.BeginTransaction();
        await repository.Add(accountForUpdatePassHash);
        await unitOfWork.Commit();
        
        // Act
        accountForUpdatePassHash.SetPasswordHash(new string('-', 60));
        await unitOfWork.BeginTransaction();
        await repository.UpdatePasswordHash(accountForUpdatePassHash);
        await unitOfWork.Commit();
        
        // Assert
        var accountFromDb = await repository.GetById(accountForUpdatePassHash.Id);
        Assert.NotNull(accountFromDb);
        Assert.Equivalent(accountForUpdatePassHash, accountFromDb);
    }

    private class UnitOfWorkAndRepoBuilder
    {
        private IDbConnection _connection = null!;
        private DbSession _session = null!;
        private readonly Mock<ILogger<PostgresRetryPolicy>> _postgresRetryPolicyLoggerMock = new();

        public (IUnitOfWork, IAccountRepository) Build()
        {
            var unitOfWork = new UnitOfWork(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            var accountRepository =
                new AccountRepository(_session, new PostgresRetryPolicy(_postgresRetryPolicyLoggerMock.Object));
            return (unitOfWork, accountRepository);
        }

        public void ConfigureConnection(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _session = new DbSession(_connection);
        }
    }
}