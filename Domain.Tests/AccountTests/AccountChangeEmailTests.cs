using Domain.AccountAggregate;
using Domain.Exceptions;
using Domain.Tests.AccountTests.Common;
using JetBrains.Annotations;
using Xunit;

namespace Domain.Tests.AccountTests;

[TestSubject(typeof(Account))]
public class AccountChangeEmailTests
{
    private readonly TestingAccountCreator _accountCreator;

    public AccountChangeEmailTests() => 
        _accountCreator = new TestingAccountCreator();

    [Theory]
    [InlineData("test@test.com")]
    [InlineData("gmail@gmail.com")]
    [InlineData("someemail@yandex.ru")]
    public void ChangeEmail_Should_Change_Email(string newEmail)
    {
        // Arrange
        var account = _accountCreator.CreateAccount();
        
        // Act
        account.ChangeEmail(newEmail);
        var actualEmail = account.Email;
        
        // Assert
        Assert.Equal(newEmail, actualEmail);
    }

    [Fact]
    public void ChangeEmail_Should_Throw_DomainException_If_Email_Is_Empty()
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act()  => account.ChangeEmail("");

        // Assert
        Assert.Throws<DomainException>(Act);
    }
    
    [Fact]
    public void ChangeEmail_Should_Throw_DomainException_If_Email_Is_Invalid()
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act()  => account.ChangeEmail("bademail.com");

        // Assert
        Assert.Throws<DomainException>(Act);
    }
}