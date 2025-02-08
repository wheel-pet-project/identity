using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Core.Domain.Services.CreateAccountService;

[TestSubject(typeof(global::Core.Domain.Services.CreateAccountService.CreateAccountService))]
public class CreateAccountServiceShould
{
    private readonly (Role role, string email, string phone, string password) _parameters = (Role.Customer,
        "email@mail.com", "+79008007060", "password");
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60), Guid.NewGuid());
    
    [Fact]
    public async Task CreateAccountIfEmailNotExistsAndPasswordCorrect()
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureAccountRepository(null);
        serviceBuilder.ConfigureHasher(new string('*', 60));
        var service = serviceBuilder.Build();

        // Act
        var creatingAccountResult = await service.CreateUser(_parameters.role, _parameters.email, _parameters.phone, 
            _parameters.password, Guid.NewGuid());

        // Assert
        Assert.True(creatingAccountResult.IsSuccess);
    }

    [Fact]
    public async Task ReturnFailureIfEmailAlreadyExists()
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureAccountRepository(_account);
        serviceBuilder.ConfigureHasher(new string('*', 60));
        var service = serviceBuilder.Build();

        // Act
        var creatingAccountResult = await service.CreateUser(_parameters.role, _parameters.email, _parameters.phone, 
            _parameters.password, Guid.NewGuid());

        // Assert
        Assert.True(creatingAccountResult.IsFailed);
    }

    [Theory]
    [InlineData("55555")] // password must be greater than 6 symbols
    [InlineData("30303030303030303030303030303030330303030")] // password must be less than 30 symbols
    public async Task ThrowValueOutOfRangeExceptionIfPasswordInvalid(string invalidPassword)
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureAccountRepository(null);
        serviceBuilder.ConfigureHasher(new string('*', 60));
        var service = serviceBuilder.Build();

        // Act
        async Task<Result<Account>> Act() =>
            await service.CreateUser(_parameters.role, _parameters.email, _parameters.phone, invalidPassword, 
                Guid.NewGuid());

        // Assert
        await Assert.ThrowsAsync<ValueOutOfRangeException>(Act);
    }
    
    private class ServiceBuilder
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IHasher> _hasherMock = new();
        
        public global::Core.Domain.Services.CreateAccountService.CreateAccountService Build() => 
            new(_accountRepositoryMock.Object, _hasherMock.Object);

        public void ConfigureAccountRepository(Account getByEmailShouldReturn) =>
            _accountRepositoryMock.Setup(x => x.GetByEmail(It.IsAny<string>(), default))
                .ReturnsAsync(getByEmailShouldReturn);

        public void ConfigureHasher(string generateHashShouldReturn) => _hasherMock
            .Setup(x => x.GenerateHash(It.IsAny<string>())).Returns(generateHashShouldReturn);
    }
}

