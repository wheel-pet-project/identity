using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Infrastructure.Interfaces.PasswordHasher;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Core.Domain.Services.UpdateAccountPasswordService;

[TestSubject(typeof(global::Core.Domain.Services.UpdateAccountPasswordService.UpdateAccountPasswordService))]
public class UpdateAccountPasswordServiceShould
{
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
    private readonly Mock<Account> _accountMock = new();
    
    [Fact]
    public void CanReturnSuccessWhenValidationReturnSuccess()
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureHasher(generateHashShouldReturn: new string('*', 60));
        var service = serviceBuilder.Build();

        // Act
        var result = service.UpdatePassword(_account, "password");

        // Assert
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public void CanChangeAccountStateWithSetNewPasswordHash()
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureHasher(generateHashShouldReturn: new string('!', 60));
        var service = serviceBuilder.Build();
        var oldPasswordHash = _account.PasswordHash;

        // Act
        service.UpdatePassword(_account, "password");

        // Assert
        Assert.NotEqual(oldPasswordHash, _account.PasswordHash);
        Assert.NotEmpty(_account.PasswordHash);
    }

    [Theory]
    [InlineData("55555")] // less than 6 symbols
    [InlineData("3131313131313131313131313131313")] // greater than 30 symbols
    public void CanThrowValueOutOfRangeExceptionIfPasswordIsInvalid(string invalidPassword)
    {
        // Arrange
        var serviceBuilder = new ServiceBuilder();
        serviceBuilder.ConfigureHasher(generateHashShouldReturn: new string('*', 60));
        var service = serviceBuilder.Build();

        // Act
        void Act() => service.UpdatePassword(_account, invalidPassword);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }
    
    private class ServiceBuilder
    {
        private readonly Mock<IHasher> _hasherMock = new();

        public global::Core.Domain.Services.UpdateAccountPasswordService.UpdateAccountPasswordService Build() =>
            new(_hasherMock.Object);

        public void ConfigureHasher(string generateHashShouldReturn) =>
            _hasherMock.Setup(x => x.GenerateHash(It.IsAny<string>())).Returns(generateHashShouldReturn);
    }
}