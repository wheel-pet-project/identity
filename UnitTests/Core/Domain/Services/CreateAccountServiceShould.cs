using Core.Domain.AccountAggregate;
using Core.Domain.Services;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Core.Domain.Services;

public class CreateAccountServiceShould
{
    private readonly (Role role, string email, string phone, string password) _parameters = (Role.Customer,
        "email@mail.com", "+79008007060", new string('*', 60));
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
    
    [Fact]
    public async Task CanCreateAccountIfEmailNotExists()
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureAccountRepository(null);
        var service = serviceBuilder.Build();

        // Act
        var creatingAccountResult =
            await service.CreateUser(_parameters.role, _parameters.email, _parameters.phone, _parameters.password);

        // Assert
        Assert.True(creatingAccountResult.IsSuccess);
    }

    [Fact]
    public async Task CanReturnFailureIfEmailAlreadyExists()
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureAccountRepository(_account);
        var service = serviceBuilder.Build();

        // Act
        var creatingAccountResult =
            await service.CreateUser(_parameters.role, _parameters.email, _parameters.phone, _parameters.password);

        // Assert
        Assert.True(creatingAccountResult.IsFailed);
    }
    
    private class ServiceBuilder
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        
        public CreateAccountService Build() => new CreateAccountService(_accountRepositoryMock.Object);

        public void ConfigureAccountRepository(Account getByEmailShouldReturn) =>
            _accountRepositoryMock.Setup(x => x.GetByEmail(It.IsAny<string>(), default))
                .ReturnsAsync(getByEmailShouldReturn);
    }
}

