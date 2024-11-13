using System;
using Domain.AccountAggregate;
using Xunit;

namespace Domain.Tests.StatusTests;

public class StatusCanSetThisStatusTests
{
    [Fact]
    public void
        StatusCanSetThisStatus_Should_Return_True_For_PendingConfirmation_Status_If_Setting_Status_Is_PendingApproval()
    {
        // Arrange
        var startStatus = Status.PendingConfirmation;
        var settingStatus = Status.PendingApproval;

        // Act
        var actual = startStatus.CanSetThisStatus(settingStatus);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void StatusCanSetThisStatus_Should_Return_True_For_PendingApproval_Status_If_Setting_Status_Is_Approved()
    {
        // Arrange
        var startStatus = Status.PendingApproval;
        var settingStatus = Status.Approved;

        // Act
        var actual = startStatus.CanSetThisStatus(settingStatus);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void StatusCanSetThisStatus_Should_Return_False_For_All_Statuses_If_Setting_Status_Is_Equal_Actual_Status()
    {
        // Arrange
        var startStatus = Status.PendingApproval;
        var settingStatus = startStatus;

        // Act
        var actual = startStatus.CanSetThisStatus(settingStatus);

        // Assert
        Assert.False(actual);
    }
    
    [Fact]
    public void 
        StatusCanSetThisStatus_Should_Return_False_For_All_Statuses_Exclude_Deleted_If_Setting_Status_Is_Deleted()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;
        var pendingApproval = Status.PendingApproval;
        var approved = Status.Approved;
        var deactivated = Status.Deactivated;
        var deleted = Status.Deleted;

        // Act
        var pendingConfirmationActualResult = pendingConfirmation.CanSetThisStatus(Status.Deleted);
        var pendingApprovalActualResult = pendingApproval.CanSetThisStatus(Status.Deleted);
        var approvedActualResult = approved.CanSetThisStatus(Status.Deleted);
        var deactivatedActualResult = deactivated.CanSetThisStatus(Status.Deleted);
        
        var deletedActualResult = deleted.CanSetThisStatus(Status.Deleted);

        // Assert
        Assert.True(pendingConfirmationActualResult);
        Assert.True(pendingApprovalActualResult);
        Assert.True(approvedActualResult);
        Assert.True(deactivatedActualResult);
        
        Assert.False(deletedActualResult);
    }
    
    [Fact]
    public void 
        StatusCanSetThisStatus_Should_Return_False_For_All_Statuses_Exclude_Deactivated_If_Setting_Status_Is_Deactivated()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;
        var pendingApproval = Status.PendingApproval;
        var approved = Status.Approved;
        var deactivated = Status.Deactivated;
        var deleted = Status.Deleted;

        // Act
        var pendingConfirmationActualResult = pendingConfirmation.CanSetThisStatus(Status.Deactivated);
        var pendingApprovalActualResult = pendingApproval.CanSetThisStatus(Status.Deactivated);
        var approvedActualResult = approved.CanSetThisStatus(Status.Deactivated);
        var deletedActualResult = deleted.CanSetThisStatus(Status.Deactivated);

        var deactivatedActualResult = deactivated.CanSetThisStatus(Status.Deactivated);
            
        // Assert
        Assert.True(pendingConfirmationActualResult);
        Assert.True(pendingApprovalActualResult);
        Assert.True(approvedActualResult);
        Assert.True(deletedActualResult);
        
        Assert.False(deactivatedActualResult);
    }

    [Fact]
    public void StatusCanSetThisStatus_Should_Throw_ArgumentNullException_When_Setting_Status_Is_Null()
    {
        // Arrange
        var startStatus = Status.PendingConfirmation;

        // Act
        void Act() => startStatus.CanSetThisStatus(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(Act);
    }

    [Fact]
    public void StatusCanSetThisStatus_Should_Throw_ArgumentOutOfRangeException_When_Setting_Status_Is_Unknown()
    {
        // Arrange
        var startStatus = Status.PendingConfirmation;
        var settingStatus = new Status("Unknown", Int32.MaxValue);

        // Act
        void Act() => startStatus.CanSetThisStatus(settingStatus);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }
}