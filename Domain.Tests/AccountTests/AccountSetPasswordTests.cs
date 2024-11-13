using Domain.AccountAggregate;
using Domain.Exceptions;
using Domain.Tests.AccountTests.Common;
using JetBrains.Annotations;
using Xunit;

namespace Domain.Tests.AccountTests;

[TestSubject(typeof(Account))]
public class AccountSetPasswordTests
{
    private readonly TestingAccountCreator _accountCreator = new();

    [Theory]
    [InlineData("$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG")]
    [InlineData("$2a$11$4327xt8asdawadwgaegchzS6lZhZkjy7NKoLpuxVS6ZN7895tgh3T")]
    public void UpdatePassword_Should_Update_Password(string password)
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        account.SetPassword(password);

        // Assert
        Assert.Equal(password, account.Password);
    }

    [Fact]
    public void UpdatePassword_Should_Throw_Exception_When_Password_Is_Empty()
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act() => account.SetPassword(string.Empty);

        // Assert
        Assert.Throws<InvalidPasswordException>(Act);
    }

    [Fact]
    public void UpdatePassword_Should_Throw_Exception_When_Password_Is_Less_Than_6_Characters()
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        void Act() => account.SetPassword("notHash");

        // Assert
        Assert.Throws<InvalidPasswordException>(Act);
    }
}