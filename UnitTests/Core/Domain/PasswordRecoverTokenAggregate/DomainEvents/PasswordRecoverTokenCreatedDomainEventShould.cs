using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

[TestSubject(typeof(PasswordRecoverTokenCreatedDomainEvent))]
public class PasswordRecoverTokenCreatedDomainEventShould
{
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60), Guid.NewGuid());

    private readonly TimeProvider _timeProvider = TimeProvider.System;

    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange
        var (accountId, recoverToken) = (Guid.NewGuid(), Guid.NewGuid());

        // Act
        var actual = new PasswordRecoverTokenCreatedDomainEvent(accountId, recoverToken);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(accountId, actual.AccountId);
        Assert.Equal(recoverToken, actual.RecoverToken);
    }

    [Fact]
    public void ThrowArgumentExceptionWhenAccountIdIsEmpty()
    {
        // Arrange
        var (emptyAccountId, recoverToken) = (Guid.Empty, Guid.NewGuid());

        // Act
        void Act()
        {
            new PasswordRecoverTokenCreatedDomainEvent(emptyAccountId, recoverToken);
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void ThrowArgumentExceptionWhenRecoverTokenIsEmpty()
    {
        // Arrange
        var (accountId, emptyRecoverToken) = (Guid.NewGuid(), Guid.Empty);

        // Act
        void Act()
        {
            new PasswordRecoverTokenCreatedDomainEvent(accountId, emptyRecoverToken);
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }
}