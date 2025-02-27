using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.AccountAggregate;

[TestSubject(typeof(Account))]
public class AccountShould
{
    [Fact]
    public void CreateAccountReturnAccountWithCorrectValues()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";

        // Act
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Assert
        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(role, account.Role);
        Assert.Equal(Status.PendingConfirmation, account.Status);
        Assert.Equal(email, account.Email);
        Assert.Equal(phone, account.Phone);
        Assert.Equal(passwordHash, account.PasswordHash);
    }

    [Fact]
    public void NotCreateAccountWhenRoleIsNullAndThrowsValueIsInvalidException()
    {
        // Arrange
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";

        // Act
        void Act()
        {
            Account.Create(null, email, phone, passwordHash, Guid.NewGuid());
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("mail@mail")]
    [InlineData("mail.com")]
    [InlineData("@mail.com")]
    public void NotCreateAccountWhenEmailIsInvalidAndThrowsValueIsInvalidException(string invalidEmail)
    {
        // Arrange
        var role = Role.Customer;
        var email = invalidEmail;
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";

        // Act
        void Act()
        {
            Account.Create(role, email, phone, passwordHash, Guid.NewGuid());
        }

        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }

    [Fact]
    public void NotCreateAccountWhenEmailIsNullAndThrowsValueIsInvalidException()
    {
        // Arrange
        var role = Role.Customer;
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";

        // Act
        void Act()
        {
            Account.Create(role, null, phone, passwordHash, Guid.NewGuid());
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("+7-900-800-70-60")]
    [InlineData("59008007060")]
    [InlineData("8900800706")]
    [InlineData("9008007060")]
    [InlineData("")]
    [InlineData(" ")]
    public void NotCreateAccountWhenPhoneIsInvalidAndThrowsValueIsInvalidException(string invalidPhone)
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = invalidPhone;
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";

        // Act
        void Act()
        {
            Account.Create(role, email, phone, passwordHash, Guid.NewGuid());
        }

        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }

    [Fact]
    public void NotCreateAccountWhenPhoneIsNullAndThrowsValueIsInvalidException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";

        // Act
        void Act()
        {
            Account.Create(role, email, null, passwordHash, Guid.NewGuid());
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Wn")] // 59 symbols, must be 60
    [InlineData("")]
    [InlineData(" ")]
    public void NotCreateAccountWhenPasswordHashIsInvalidAndThrowsValueOutOfRangeException(
        string invalidPassword)
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = invalidPassword;

        // Act
        void Act()
        {
            Account.Create(role, email, phone, passwordHash, Guid.NewGuid());
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void NotCreateAccountWhenInvalidPasswordHashIsNullAndThrowsInvalidPasswordHashException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";

        // Act
        void Act()
        {
            Account.Create(role, email, phone, null, Guid.NewGuid());
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void SetStatusMustSetNewAccountStatus()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        account.SetStatus(Status.PendingApproval);

        // Assert
        Assert.Equal(Status.PendingApproval, account.Status);
    }

    [Fact]
    public void SetStatusWhenNewStatusIsNullMustThrowsValueOutOfRangeException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetStatus(null);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void SetStatusWhenNewStatusViolationDomainRulesMustThrowsDomainRulesViolationException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetStatus(Status.Approved);
        }

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }

    [Fact]
    public void SetRoleMustSetNewAccountRole()
    {
        // Arrange
        var role = Role.Maintenance;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        account.SetRole(Role.Support);

        // Assert
        Assert.Equal(Role.Support, account.Role);
    }

    [Fact]
    public void SetRoleWhenNewRoleIsNullMustThrowsValueOutOfRangeException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetStatus(null);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void SetRoleWhenNewRoleViolationDomainRulesMustThrowsDomainRulesViolationException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetRole(Role.Support);
        }

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }

    [Fact]
    public void SetEmailMustSetNewAccountEmail()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        var newEmail = "newemail@test.com";

        // Act
        account.SetEmail(newEmail);

        // Assert
        Assert.Equal(newEmail, account.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("mail@mail")]
    [InlineData("mail.com")]
    [InlineData("@mail.com")]
    public void SetEmailWhenNewEmailIsInvalidMustThrowsValueIsInvalidException(string invalidEmail)
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetEmail(invalidEmail);
        }

        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }

    [Fact]
    public void SetEmailWhenNewEmailIsNullMustThrowsValueIsInvalidException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetEmail(null);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void SetPhoneMustSetNewAccountPhone()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        var newPhone = "89008007060";

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
    [InlineData("")]
    [InlineData(" ")]
    public void SetPhoneWhenNewPhoneIsInvalidMustThrowsValueIsInvalidException(string invalidPhone)
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetPhone(invalidPhone);
        }

        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }

    [Fact]
    public void SetPhoneWhenNewPhoneIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetPhone(null);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void SetPasswordMustSetNewAccountPasswordHash()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        var newPasswordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Fht";

        // Act
        account.SetPasswordHash(newPasswordHash);

        // Assert
        Assert.Equal(newPasswordHash, account.PasswordHash);
    }

    [Theory]
    [InlineData("$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Wn")] // 59 symbols, must be 60
    [InlineData("")]
    [InlineData(" ")]
    public void SetPasswordWhenNewPasswordHashIsInvalidMustThrowsValueOutOfRangeException(string invalidPassword)
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetPasswordHash(invalidPassword);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void SetPasswordWhenNewPasswordHashIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            account.SetPasswordHash(null);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("666666")] // min password length - 6 symbols
    [InlineData("303030303030303030303030303030")] // max password length - 30 symbols
    public void ValidateNotHashedPasswordMustReturnTrueIfNotHashedPasswordCorrect(string password)
    {
        // Arrange

        // Act
        var actual = Account.ValidateNotHashedPassword(password);

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [InlineData("55555")] // less than 6 symbols
    [InlineData("3131313131313131313131313131313")] // greater than 30 symbols
    public void ValidateNotHashedPasswordMustReturnFalseIfNotHashedPasswordInvalid(string invalidPassword)
    {
        // Arrange

        // Act
        var actual = Account.ValidateNotHashedPassword(invalidPassword);

        // Assert
        Assert.False(actual);
    }
}