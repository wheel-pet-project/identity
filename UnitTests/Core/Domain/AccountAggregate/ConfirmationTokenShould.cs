using Core.Domain.AccountAggregate;
using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Xunit;

namespace UnitTests.Core.Domain.AccountAggregate;

public class ConfirmationTokenShould
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@mail.com", "+79008007060", new string('*', 60));
    private readonly string _confirmationTokenHash = new string('*', 60);
    
    [Fact]
    public void CreateInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var confirmationToken = ConfirmationToken.Create(_account.Id, _confirmationTokenHash);

        // Assert
        Assert.Equal(_account.Id, confirmationToken.AccountId);
        Assert.Equal(_confirmationTokenHash, confirmationToken.ConfirmationTokenHash);
    }

    [Fact]
    public void NotCreateWhenAccountIdIsEmptyAndShouldThrowsValueIsRequiredException()
    {
        // Arrange


        // Act
        void Act() => ConfirmationToken.Create(Guid.Empty, _confirmationTokenHash);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void NotCreateWhenConfirmationTokenHashIsNullAndShouldThrowsValueIsRequiredException()
    {
        // Arrange


        // Act
        void Act() => ConfirmationToken.Create(_account.Id, null);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void NotCreateWhenConfirmationTokenHashIsInvalidAndShouldThrowsValueIsRequiredException()
    {
        // Arrange


        // Act
        void Act() => ConfirmationToken.Create(_account.Id, new string('*', 59)); // hash length must 60 

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }
}