using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Xunit;

namespace Core.Tests.Domain.RefreshTokenAggregate;

public class RefreshTokenShould
{
    private const string PasswordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
    
    [Fact]
    public void CreateRefreshTokenWithCorrectValues()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash);

        // Act
        var refreshToken = RefreshToken.Create(account);

        // Assert
        Assert.Equal(account.Id, refreshToken.AccountId);
        Assert.True(refreshToken.IssueDateTime < refreshToken.ExpiresAt);
        Assert.False(refreshToken.IsRevoked);
    }

    [Fact]
    public void CreateRefreshTokenWithAccountIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange

        // Act
        void Act() => RefreshToken.Create(null);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void IsValidWithForNewRefreshTokenMustReturnTrue()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash);

        // Act
        var refreshToken = RefreshToken.Create(account);

        // Assert
        Assert.True(refreshToken.IsValid());
    }

    [Fact]
    public void RevokeMustChangeIsRevokedPropertyToTrueAndIsValidMustReturnFalse()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash);
        var refreshToken = RefreshToken.Create(account);
        
        // Act
        refreshToken.Revoke();

        // Assert
        Assert.True(refreshToken.IsRevoked);
        Assert.False(refreshToken.IsValid());
    }
}