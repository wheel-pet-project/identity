using Domain.AccountAggregate;
using JetBrains.Annotations;
using Xunit;

namespace Domain.Tests.RoleTests;

[TestSubject(typeof(Role))]
public class RoleEqOperatorsTests
{
    [Fact]
    public void Eq_Should_Return_True_If_Roles_Is_Identical()
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
    public void Eq_Should_Return_False_If_Roles_Is_Not_Identical()
    {
        // Arrange
        var roleA = Role.Customer;
        var roleB = Role.Administrator;

        // Act
        var actual = roleA == roleB;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void NotEq_Should_Return_False_If_Roles_Is_Identical()
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
    public void NotEq_Should_Return_True_If_Roles_Is_Not_Identical()
    {
        // Arrange
        var roleA = Role.Customer;
        var roleB = Role.Administrator;

        // Act
        var actual = roleA != roleB;

        // Assert
        Assert.True(actual);
    }
}