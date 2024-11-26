using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using Xunit;

namespace Core.Tests.Domain.AccountAggregate;

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
        var account = Account.Create(role, email, phone, passwordHash);

        // Assert
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
        void Act() => Account.Create(role: null, email, phone, passwordHash);

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
        void Act() => Account.Create(role, email, phone, passwordHash);

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
        void Act() => Account.Create(role, email: null, phone, passwordHash);

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
        void Act() => Account.Create(role, email, phone, passwordHash);

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
        void Act() => Account.Create(role, email, phone: null, passwordHash);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Wn")] // 59 symbols, must be 60
    [InlineData("")]
    [InlineData(" ")]
    public void NotCreateAccountWhenPasswordHashIsInvalidAndThrowsInvalidPasswordHashException(
        string invalidPassword)
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = invalidPassword;

        // Act
        void Act() => Account.Create(role, email, phone, passwordHash);

        // Assert
        Assert.Throws<ValueIsInvalidException>(Act);
    }
    
    [Fact]
    public void NotCreateAccountWhenInvalidPasswordHashIsNullAndThrowsInvalidPasswordHashException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";

        // Act
        void Act() => Account.Create(role, email, phone, passwordHash: null);

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
        var account = Account.Create(role, email, phone, passwordHash);

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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetStatus(null);

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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetStatus(Status.Approved);

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
        var account = Account.Create(role, email, phone, passwordHash);

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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetStatus(null);

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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetRole(Role.Support);

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
        var account = Account.Create(role, email, phone, passwordHash);
        
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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetEmail(invalidEmail);

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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetEmail(null);

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
        var account = Account.Create(role, email, phone, passwordHash);
        
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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetPhone(invalidPhone);

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
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetPhone(null);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void SetPasswordMustSetNewAccountPassword()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash);
        
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
    public void SetPasswordWhenNewPasswordIsInvalidMustThrowsValueOutOfRangeException(string invalidPassword)
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash);

        // Act
        void Act() => account.SetPasswordHash(invalidPassword);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }
    
    [Fact]
    public void SetPasswordWhenNewPasswordIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange
        var role = Role.Customer;
        var email = "test@test.com";
        var phone = "+79008007060";
        var passwordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
        var account = Account.Create(role, email, phone, passwordHash);
        
        // Act
        void Act() => account.SetPasswordHash(null);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}