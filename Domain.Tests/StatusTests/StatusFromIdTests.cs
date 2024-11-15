using Domain.AccountAggregate;
using Xunit;

namespace Domain.Tests.StatusTests;

public class StatusFromIdTests
{
    [Fact]
    public void FromId_Should_Return_Approved_Status()
    {
        // Arrange
        var expected = Status.Approved;
        var statusId = Status.Approved.Id;

        // Act
        var actual = Status.FromId(statusId);

        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void FromId_Should_Return_PendingConfirmation_Status()
    {
        // Arrange
        var expected = Status.PendingConfirmation;
        var statusId = Status.PendingConfirmation.Id;

        // Act
        var actual = Status.FromId(statusId);

        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void FromId_Should_Return_PendingApproval_Status()
    {
        // Arrange
        var expected = Status.PendingApproval;
        var statusId = Status.PendingApproval.Id;

        // Act
        var actual = Status.FromId(statusId);

        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void FromId_Should_Return_Deactivated_Status()
    {
        // Arrange
        var expected = Status.Deactivated;
        var statusId = Status.Deactivated.Id;

        // Act
        var actual = Status.FromId(statusId);

        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void FromId_Should_Return_Deleted_Status()
    {
        // Arrange
        var expected = Status.Deleted;
        var statusId = Status.Deleted.Id;

        // Act
        var actual = Status.FromId(statusId);

        // Assert
        Assert.Equal(expected, actual);
    }
}