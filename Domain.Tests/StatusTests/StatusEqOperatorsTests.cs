using Domain.AccountAggregate;
using Xunit;

namespace Domain.Tests.StatusTests;

public class StatusEqOperatorsTests
{
    [Fact]
    public void Eq_Should_Return_True_If_Statuses_Is_Identical()
    {
        // Arrange
        var statusA = Status.Confirmed;
        var statusB = Status.Confirmed;

        // Act
        var actual = statusA == statusB;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Eq_Should_Return_False_If_Statuses_Is_Not_Identical()
    {
        // Arrange
        var statusA = Status.Confirmed;
        var statusB = Status.Deleted;

        // Act
        var actual = statusA == statusB;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void NotEq_Should_Return_False_If_Statuses_Is_Identical()
    {
        // Arrange
        var statusA = Status.Confirmed;
        var statusB = Status.Confirmed;

        // Act
        var actual = statusA != statusB;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void NotEq_Should_Return_True_If_Statuses_Is_Not_Identical()
    {
        // Arrange
        var statusA = Status.Confirmed;
        var statusB = Status.Deleted;

        // Act
        var actual = statusA != statusB;

        // Assert
        Assert.True(actual);
    }
}