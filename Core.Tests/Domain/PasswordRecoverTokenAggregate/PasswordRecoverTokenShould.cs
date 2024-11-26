using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Xunit;

namespace Core.Tests.Domain.PasswordRecoverTokenAggregate;

public class PasswordRecoverTokenShould
{
    private const string PasswordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
    private const string PasswordRecoverTokenHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Klq";

    [Fact]
    public void CreatePasswordRecoverTokenWithCorrectValues()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash);

        // Act
        var passwordRecoverToken = PasswordRecoverToken.Create(PasswordRecoverTokenHash, account);

        // Assert
        Assert.Equal(PasswordRecoverTokenHash, passwordRecoverToken.RecoverTokenHash);
        Assert.Equal(account.Id, passwordRecoverToken.AccountId);
        Assert.True(passwordRecoverToken.ExpiresAt > DateTime.UtcNow);
        Assert.False(passwordRecoverToken.IsAlreadyApplied);
    }

    [Fact]
    public void CreatePasswordRecoverTokenWhenAccountIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange

        // Act
        void Act() => PasswordRecoverToken.Create(PasswordRecoverTokenHash, null);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Wn")] // 59 symbols, must be 60
    public void CreatePasswordRecoverTokenWhenPasswordHashIsInvalidMustThrowsValueOutOfRangeException(string invalidRecoverTokenHash)
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash);

        // Act
        void Act() => PasswordRecoverToken.Create(invalidRecoverTokenHash, account);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void IsValidForNewPasswordRecoverTokenMustReturnTrue()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash);
        var passwordRecoverToken = PasswordRecoverToken.Create(PasswordRecoverTokenHash, account);
        
        // Act
        var result = passwordRecoverToken.IsValid();

        // Assert
        Assert.True(result);
    }
}