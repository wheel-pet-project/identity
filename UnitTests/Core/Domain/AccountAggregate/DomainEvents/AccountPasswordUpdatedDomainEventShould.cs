using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.AccountAggregate.DomainEvents;

[TestSubject(typeof(AccountPasswordUpdatedDomainEvent))]
public class AccountPasswordUpdatedDomainEventShould
{
    [Fact]
    public void CanCreateAccountPasswordUpdatedEventWithCorrectValues()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var actual = new AccountPasswordUpdatedDomainEvent(accountId);

        // Assert
        Assert.Equal(accountId, actual.AccountId);
    }

    [Fact]
    public void CanThrowValueIsRequiredExceptionWhenAccountIdIsEmpty()
    {
        // Arrange
        var accountId = Guid.Empty;

        // Act
        void Act() => new AccountPasswordUpdatedDomainEvent(accountId);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}