using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Core.Domain.AccountAggregate;

[TestSubject(typeof(Status))]
public class StatusShould
{
    [Fact]
    public void FromNameMustReturnCorrectStatus()
    {
        // Arrange
        var name = Status.PendingConfirmation.Name;

        // Act
        var status = Status.FromName(name);

        // Assert
        Assert.Equal(Status.PendingConfirmation, status);
    }
    
    [Fact]
    public void FromIdShouldReturnCorrectStatus()
    {
        // Arrange
        var statusId = Status.Approved.Id;

        // Act
        var role = Status.FromId(statusId);

        // Assert
        Assert.Equal(Status.Approved, role);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void FromIdWhenIdIsInvalidMustThrowsValueIsRequiredException(int invalidRoleId)
    {
        // Arrange

        // Act
        void Act() => Status.FromId(invalidRoleId);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }
    
    [Fact]
    public void IsCanAuthorizeMustReturnTrueForPendingApprovalAndApprovedStatuses()
    {
        // Arrange
        var pendingApprovalStatus = Status.Approved;
        var approvedStatus = Status.Approved;

        // Act
        var pendingApprovalResult = pendingApprovalStatus.CanBeAuthorize();
        var approvedResult = approvedStatus.CanBeAuthorize();

        // Assert
        Assert.True(pendingApprovalResult);
        Assert.True(approvedResult);
    }

    [Fact]
    public void 
        IsCanAuthorizeShouldReturnFalseForAllStatusesExcludingPendingApprovalAndApprovedStatuses()
    {
        // Arrange
        var deactivated = Status.Deactivated;
        var deleted = Status.Deleted;
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        var deactivatedActualResult = deactivated.CanBeAuthorize();
        var deletedActualResult = deleted.CanBeAuthorize();
        var pendingConfirmationActualResult = pendingConfirmation.CanBeAuthorize();

        // Assert
        Assert.False(deactivatedActualResult);
        Assert.False(deletedActualResult);
        Assert.False(pendingConfirmationActualResult);
    }
    
    [Fact]
    public void CanBeChangedToThisStatusWhenPotentialStatusIsValidForDomainStateMachineMustReturnTrue()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;
        var pendingApproval = Status.PendingApproval;
        var approved = Status.Approved;
        var deactivated = Status.Deactivated;
        
        // Act
        var pendingConfirmationToPendingApprovalResult = pendingConfirmation.CanBeChangedToThisStatus(Status.PendingApproval);
        var pendingApprovalToApprovedResult = pendingApproval.CanBeChangedToThisStatus(Status.Approved);
        var approvedToDeactivatedResult = approved.CanBeChangedToThisStatus(Status.Deactivated);
        var approvedToPendingApprovalResult = approved.CanBeChangedToThisStatus(Status.PendingApproval);
        var deactivatedToDeletedResult = deactivated.CanBeChangedToThisStatus(Status.Deleted);
        var deactivatedToApprovedResult = deactivated.CanBeChangedToThisStatus(Status.Approved);

        // Assert
        Assert.True(pendingConfirmationToPendingApprovalResult);
        Assert.True(pendingApprovalToApprovedResult);
        Assert.True(approvedToDeactivatedResult);
        Assert.True(approvedToPendingApprovalResult);
        Assert.True(deactivatedToDeletedResult);
        Assert.True(deactivatedToApprovedResult);
    }

    [Fact]
    public void CanBeChangedToThisStatusWhenPotentialStatusIsInvalidForDomainStateMachineMustReturnFalse()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;
        var pendingApproval = Status.PendingApproval;
        var approved = Status.Approved;
        var deactivated = Status.Deactivated;

        // Act
        var pendingConfirmationToApprovedResult = pendingConfirmation.CanBeChangedToThisStatus(Status.Approved);
        var pendingApprovalToPendingConfirmationResult = pendingApproval.CanBeChangedToThisStatus(Status.PendingConfirmation);
        var approvedToPendingConfirmationResult = approved.CanBeChangedToThisStatus(Status.PendingConfirmation);
        var pendingConfirmationToDeactivatedResult = pendingConfirmation.CanBeChangedToThisStatus(Status.Deactivated);
        var pendingApprovalToDeactivatedResult = pendingApproval.CanBeChangedToThisStatus(Status.Deactivated);
        var deactivatedToPendingConfirmationResult = deactivated.CanBeChangedToThisStatus(Status.PendingConfirmation);
        

        // Assert
        Assert.False(pendingConfirmationToApprovedResult);
        Assert.False(pendingApprovalToPendingConfirmationResult);
        Assert.False(approvedToPendingConfirmationResult);
        Assert.False(pendingConfirmationToDeactivatedResult);
        Assert.False(pendingApprovalToDeactivatedResult);
        Assert.False(deactivatedToPendingConfirmationResult);
    }
    
    [Fact]
    public void CanBeChangedToThisStatusWhenPotentialStatusIsEqualCurrentStatusMustReturnFalse()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        var result = pendingConfirmation.CanBeChangedToThisStatus(Status.PendingConfirmation);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanBeChangedToThisStatusWhenPotentialStatusIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        void Act() => pendingConfirmation.CanBeChangedToThisStatus(null);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void EqOperatorForIdenticalStatusesMustReturnTrue()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        var result = pendingConfirmation == Status.PendingConfirmation;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqOperatorForDifferentStatusesMustReturnFalse()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        var result = pendingConfirmation == Status.Approved;

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void NotEqOperatorForIdenticalStatusesMustReturnFalse()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        var result = pendingConfirmation != Status.PendingConfirmation;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NotEqOperatorForDifferentStatusesMustReturnTrue()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        var result = pendingConfirmation != Status.Approved;

        // Assert
        Assert.True(result);
    }
}