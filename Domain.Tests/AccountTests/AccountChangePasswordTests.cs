using Domain.AccountAggregate;
using Domain.Exceptions;
using Domain.Tests.AccountTests.Common;
using JetBrains.Annotations;
using Xunit;

namespace Domain.Tests.AccountTests;

[TestSubject(typeof(Account))]
public class AccountChangePasswordTests
{
    private TestingAccountCreator _accountCreator;

    public AccountChangePasswordTests() => 
        _accountCreator = new TestingAccountCreator();

    [Theory]
    [InlineData("password")]
    [InlineData("Password123")]
    [InlineData("Password123*\"![]")]
    public void ChangePassword_Should_Change_Password(string password)
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        account.ChangePassword(password);

        // Assert
        Assert.Equal(password, account.Password);
    }

    [Fact]
    public void ChangePassword_Should_Throw_Exception_When_Password_Is_Empty()
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act() => account.ChangePassword(string.Empty);

        // Assert
        Assert.Throws<DomainException>(Act);
    }

    [Fact]
    public void ChangePassword_Should_Throw_Exception_When_Password_Is_Less_Than_6_Characters()
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act() => account.ChangePassword("55555");

        // Assert
        Assert.Throws<DomainException>(Act);
    }
}