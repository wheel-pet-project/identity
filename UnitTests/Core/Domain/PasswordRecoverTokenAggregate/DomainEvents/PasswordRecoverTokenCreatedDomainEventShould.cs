using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

[TestSubject(typeof(PasswordRecoverTokenCreatedDomainEvent))]
public class PasswordRecoverTokenCreatedDomainEventShould
{
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
    
    [Fact]
    public void CreateDomainEvent()
    {
        // Arrange
        var expEventId = Guid.NewGuid();
        
        var hash = new string('*', 60);
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, hash);

        // Act
        passwordRecoverToken.AddCreatedDomainEvent(expEventId, "email@email.com");
        
        // Assert
        Assert.NotNull(passwordRecoverToken.DomainEvents[0]);
    }

    [Fact]
    public void CanThrowValueIsRequiredWhenEventIdIsEmpty()
    {
        // Arrange
        var eventId = Guid.Empty;
        var hash = new string('*', 60);
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, hash);

        // Act
        void Act() => passwordRecoverToken.AddCreatedDomainEvent(eventId, "email@email.com");

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CanThrowValueIsRequiredWhenEmailIsInvalid(string email)
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var hash = new string('*', 60);
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, hash);

        // Act
        void Act() => passwordRecoverToken.AddCreatedDomainEvent(eventId, email);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
}