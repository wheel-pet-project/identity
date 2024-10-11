using Domain.AccountAggregate;
using Domain.Exceptions;
using Domain.Tests.AccountTests.Common;
using JetBrains.Annotations;
using Xunit;

namespace Domain.Tests.AccountTests;

[TestSubject(typeof(Account))]
public class AccountSetEmailTests
{
    private readonly TestingAccountCreator _accountCreator = new();

    [Theory]
    [InlineData("test@test.com")]
    [InlineData("gmail@gmail.com")]
    [InlineData("someemail@yandex.ru")]
    public void SetEmail_Should_Update_Email(string newEmail)
    {
        // Arrange
        var account = _accountCreator.CreateAccount();
        
        // Act
        account.SetEmail(newEmail);
        var actualEmail = account.Email;
        
        // Assert
        Assert.Equal(newEmail, actualEmail);
    }

    [Fact]
    public void SetEmail_Should_Throw_DomainException_If_Email_Is_Empty()
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act()  => account.SetEmail("");

        // Assert
        Assert.Throws<DomainException>(Act);
    }
    
    [Theory]
    [InlineData("bademail.com")]
    [InlineData("gmail@com")]
    [InlineData("some.email@yandexru")]
    public void SetEmail_Should_Throw_DomainException_If_Email_Is_Invalid(string newEmail)
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act()  => account.SetEmail(newEmail);

        // Assert
        Assert.Throws<DomainException>(Act);
    }
}