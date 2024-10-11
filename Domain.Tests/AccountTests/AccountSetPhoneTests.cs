using Domain.Exceptions;
using Domain.Tests.AccountTests.Common;
using Xunit;

namespace Domain.Tests.AccountTests;

public class AccountSetPhoneTests
{
    private readonly TestingAccountCreator _accountCreator = new();
    
    [Theory]
    [InlineData("+79008007060")]
    [InlineData("79008007060")]
    [InlineData("89008007060")]
    public void SetPhone_Should_Update_Phone(string newPhone)
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        account.SetPhone(newPhone);

        // Assert
        Assert.Equal(newPhone, account.Phone);
    }

    [Theory]
    [InlineData("+7-900-800-70-60")]
    [InlineData("59008007060")]
    [InlineData("8900800706")]
    [InlineData("9008007060")]
    public void SetPhone_Should_Throw_DomainException_If_Phone_Is_Not_Valid(string newPhone)
    {
        // Arrange
        var account = _accountCreator.CreateAccount();

        // Act
        var act = () => account.SetPhone(newPhone);

        // Assert
        Assert.Throws<DomainException>(act);
    }
}