using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.PublicExceptions;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.AccountAggregate;

[TestSubject(typeof(Role))]
public class RoleShould
{
    [Fact]
    public void FromNameMustReturnCorrectRole()
    {
        // Arrange
        var roleName = Role.Customer.Name;

        // Act
        var role = Role.FromName(roleName);

        // Assert
        Assert.Equal(Role.Customer, role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid role name")]
    public void FromNameWhenNameIsInvalidMustThrowsValueIsRequiredException(string invalidRoleName)
    {
        // Arrange

        // Act
        void Act()
        {
            Role.FromName(invalidRoleName);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void FromIdMustReturnCorrectRole()
    {
        // Arrange
        var roleId = Role.Customer.Id;

        // Act
        var role = Role.FromId(roleId);

        // Assert
        Assert.Equal(Role.Customer, role);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void FromIdWhenIdIsInvalidMustThrowsValueIsRequiredException(int invalidId)
    {
        // Arrange

        // Act
        void Act()
        {
            Role.FromId(invalidId);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }


    [Fact]
    public void EqOperatorMustReturnTrueWhenRolesIsIdentical()
    {
        // Arrange
        var roleA = Role.Customer;
        var roleB = Role.Customer;

        // Act
        var actual = roleA == roleB;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void EqOperatorMustReturnTrueWhenRolesIsDifferent()
    {
        // Arrange
        var roleA = Role.Customer;
        var roleB = Role.Admin;

        // Act
        var actual = roleA == roleB;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void NotEqOperatorMustReturnTrueWhenRolesIsDifferent()
    {
        // Arrange
        var roleA = Role.Customer;
        var roleB = Role.Customer;

        // Act
        var actual = roleA != roleB;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void NotEqOperatorMustReturnFalseWhenRolesIsIdentical()
    {
        // Arrange
        var roleA = Role.Customer;
        var roleB = Role.Admin;

        // Act
        var actual = roleA != roleB;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void BeChangedToThisRoleWhenEmployeeRoleChangesToAnotherEmployeeRoleMustReturnTrue()
    {
        // Arrange
        var role = Role.Support;

        // Act
        var result = role.CanBeChangedToThisRole(Role.Maintenance);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BeChangedToThisRoleWhenEmployeeRoleChangesToCustomerRoleMustReturnFalse()
    {
        // Arrange
        var role = Role.Support;

        // Act
        var result = role.CanBeChangedToThisRole(Role.Customer);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BeChangedToThisRoleWhenCustomerRoleChangesToEmployeeRoleMustReturnFalse()
    {
        // Arrange
        var role = Role.Customer;

        // Act
        var result = role.CanBeChangedToThisRole(Role.Admin);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BeChangedToThisRoleWhenPotentialRoleIsNullMustThrowValueIsRequiredException()
    {
        // Arrange
        var role = Role.Support;

        // Act
        void Act()
        {
            role.CanBeChangedToThisRole(null);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}