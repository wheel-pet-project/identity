using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Xunit;

namespace UnitTests.Core.Domain.ConfirmationTokenAggregate;

public class ConfirmationTokenShould
{
    [Fact]
    public void CreateConfirmationTokenWithCorrectValues()
    {
        // Arrange
        var hash = new string('*', 60);
        var accountId = Guid.NewGuid();

        // Act
        var confirmationToken = ConfirmationToken.Create(accountId, hash);

        // Assert
        Assert.Equal(accountId, confirmationToken.AccountId);
        Assert.Equal(hash, confirmationToken.ConfirmationTokenHash);
    }

    [Fact]
    public void CreateConfirmationTokenWhenTokenHashIsInvalidMustThrowsValueOutOfRangeException()
    {
        // Arrange
        var hash = new string('*', 59); // must be == 60
        var accountId = Guid.NewGuid();

        // Act
        void Act() => ConfirmationToken.Create(accountId, hash);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void CreateConfirmationTokenWhenTokenHashIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        void Act() => ConfirmationToken.Create(accountId, null);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void CreateConfirmationTokenWhenAccountIdIsInvalidMustThrowsValueIsRequiredException()
    {
        // Arrange
        var hash = new string('*', 60); // must be == 60
        var accountId = Guid.Empty;

        // Act
        void Act() => ConfirmationToken.Create(accountId, hash);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}