using Domain.AccountAggregate;
using Xunit;

namespace Domain.Tests.StatusTests;

public class StatusCanAuthorizeTests
{
    [Fact]
    public void StatusCanAuthorize_Should_Return_True_For_Approved_Status()
    {
        // Arrange
        var status = Status.Approved;

        // Act
        var actual = status.CanAuthorize();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void StatusCanAuthorize_Should_Return_True_For_PendingApproval_Status()
    {
        // Arrange
        var status = Status.PendingApproval;

        // Act
        var actual = status.CanAuthorize();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void
        StatusCanAuthorize_Should_Return_False_For_All_Statuses_Excluding_Approved_And_PendingApproval_Statuses()
    {
        // Arrange
        var deactivated = Status.Deactivated;
        var deleted = Status.Deleted;
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        var deactivatedActualResult = deactivated.CanAuthorize();
        var deletedActualResult = deleted.CanAuthorize();
        var pendingConfirmationActualResult = pendingConfirmation.CanAuthorize();

        // Assert
        Assert.False(deactivatedActualResult);
        Assert.False(deletedActualResult);
        Assert.False(pendingConfirmationActualResult);
    }
}