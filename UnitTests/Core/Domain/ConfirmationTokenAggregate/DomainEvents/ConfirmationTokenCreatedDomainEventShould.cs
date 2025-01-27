using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.ConfirmationTokenAggregate.DomainEvents;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.ConfirmationTokenAggregate.DomainEvents;

[TestSubject(typeof(ConfirmationTokenCreatedDomainEvent))]
public class ConfirmationTokenCreatedDomainEventShould
{
    [Fact]
    public void CreateDomainEvent()
    {
        // Arrange
        var expEventId = Guid.NewGuid();
        
        var hash = new string('*', 60);
        var accountId = Guid.NewGuid();
        var confirmationToken = ConfirmationToken.Create(accountId, hash);

        // Act
        confirmationToken.AddCreatedDomainEvent(expEventId, "email@email.com");
        
        // Assert
        Assert.NotNull(confirmationToken.DomainEvents[0]);
    }

    [Fact]
    public void ThrowValueIsRequiredWhenEventIdIsEmpty()
    {
        // Arrange
        var eventId = Guid.Empty;
        var hash = new string('*', 60);
        var accountId = Guid.NewGuid();
        var confirmationToken = ConfirmationToken.Create(accountId, hash);

        // Act
        void Act() => confirmationToken.AddCreatedDomainEvent(eventId, "email@email.com");

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowValueIsRequiredWhenEmailIsInvalid(string email)
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var hash = new string('*', 60);
        var accountId = Guid.NewGuid();
        var confirmationToken = ConfirmationToken.Create(accountId, hash);

        // Act
        void Act() => confirmationToken.AddCreatedDomainEvent(eventId, email);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}