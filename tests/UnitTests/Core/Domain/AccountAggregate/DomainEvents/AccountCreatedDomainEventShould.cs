using Core.Domain.AccountAggregate.DomainEvents;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.AccountAggregate.DomainEvents;

[TestSubject(typeof(AccountCreatedDomainEvent))]
public class AccountCreatedDomainEventShould
{
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Guid _confirmationToken = Guid.NewGuid();
    private readonly string _emailAddress = "email@test.com";
    private readonly string _phone = "+79605678990";

    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var domainEvent = new AccountCreatedDomainEvent(_accountId, _emailAddress, _phone, _confirmationToken);

        // Assert
        Assert.Equal(_accountId, domainEvent.AccountId);
        Assert.Equal(_emailAddress, domainEvent.Email);
        Assert.Equal(_phone, domainEvent.Phone);
        Assert.Equal(_confirmationToken, domainEvent.ConfirmationToken);
    }

    [Fact]
    public void ThrowArgumentExceptionIfAccountIdIsEmpty()
    {
        // Arrange

        // Act
        void Act() => new AccountCreatedDomainEvent(Guid.Empty, _emailAddress, _phone, _confirmationToken);

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void ThrowArgumentExceptionIfEmailAddressIsEmpty()
    {
        // Arrange

        // Act
        void Act() => new AccountCreatedDomainEvent(_accountId, " ", _phone, _confirmationToken);

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void ThrowArgumentExceptionIfPhoneNumberIsEmpty()
    {
        // Arrange

        // Act
        void Act() => new AccountCreatedDomainEvent(_accountId, _emailAddress, " ", _confirmationToken);

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void ThrowArgumentExceptionIfConfirmationTokenIsEmpty()
    {
        // Arrange

        // Act
        void Act() => new AccountCreatedDomainEvent(_accountId, _emailAddress, _phone, Guid.Empty);

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }
}