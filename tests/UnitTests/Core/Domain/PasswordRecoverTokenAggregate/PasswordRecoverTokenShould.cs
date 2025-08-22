using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.PublicExceptions;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.PasswordRecoverTokenAggregate;

[TestSubject(typeof(PasswordRecoverToken))]
public class PasswordRecoverTokenShould
{
    private const string PasswordHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG";
    private const string PasswordRecoverTokenHash = "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Klq";
    private readonly TimeProvider _timeProvider = TimeProvider.System;

    [Fact]
    public void CreatePasswordRecoverTokenWithCorrectValues()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash, Guid.NewGuid());

        // Act
        var passwordRecoverToken = PasswordRecoverToken.Create(account, Guid.NewGuid(), PasswordRecoverTokenHash,
            _timeProvider);

        // Assert
        Assert.NotEqual(Guid.Empty, passwordRecoverToken.Id);
        Assert.Equal(PasswordRecoverTokenHash, passwordRecoverToken.RecoverTokenHash);
        Assert.Equal(account.Id, passwordRecoverToken.AccountId);
        Assert.True(passwordRecoverToken.ExpiresAt > DateTime.UtcNow);
        Assert.False(passwordRecoverToken.IsAlreadyApplied);
    }

    [Fact]
    public void CreatePasswordRecoverTokenWhenAccountIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange

        // Act
        void Act()
        {
            PasswordRecoverToken.Create(null, Guid.NewGuid(), PasswordRecoverTokenHash, _timeProvider);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2Wn")] // 59 symbols, must be 60
    public void CreatePasswordRecoverTokenWhenPasswordHashIsInvalidMustThrowsValueOutOfRangeException(
        string invalidRecoverTokenHash)
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            PasswordRecoverToken.Create(account, Guid.NewGuid(), invalidRecoverTokenHash, _timeProvider);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void CreatePasswordRecoverTokenWhenPasswordHashIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash, Guid.NewGuid());

        // Act
        void Act()
        {
            PasswordRecoverToken.Create(account, Guid.NewGuid(), null, _timeProvider);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void AddDomainEvent()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash, Guid.NewGuid());


        // Act
        var passwordRecoverToken =
            PasswordRecoverToken.Create(account, Guid.NewGuid(), PasswordRecoverTokenHash, _timeProvider);

        // Assert
        Assert.True(passwordRecoverToken.DomainEvents.Any());
    }

    [Fact]
    public void IsValidForNewPasswordRecoverTokenMustReturnTrue()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash, Guid.NewGuid());
        var passwordRecoverToken =
            PasswordRecoverToken.Create(account, Guid.NewGuid(), PasswordRecoverTokenHash, _timeProvider);

        // Act
        var result = passwordRecoverToken.IsValid(_timeProvider);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ApplyMustMakeTokenApplied()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "email@mail.com", "+79008007060", PasswordHash, Guid.NewGuid());
        var passwordRecoverToken =
            PasswordRecoverToken.Create(account, Guid.NewGuid(), PasswordRecoverTokenHash, _timeProvider);

        // Act
        passwordRecoverToken.Apply();

        // Assert
        var result = passwordRecoverToken.IsValid(_timeProvider);
        Assert.False(result);
    }
}