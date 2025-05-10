using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.InternalExceptions;
using Core.Domain.SharedKernel.Exceptions.PublicExceptions;
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
        var statusId = Status.Confirmed.Id;

        // Act
        var role = Status.FromId(statusId);

        // Assert
        Assert.Equal(Status.Confirmed, role);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void FromIdWhenIdIsInvalidMustThrowsValueIsRequiredException(int invalidRoleId)
    {
        // Arrange

        // Act
        void Act()
        {
            Status.FromId(invalidRoleId);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void IsCanAuthorizeMustReturnTrueForPendingApprovalAndConfirmedStatuses()
    {
        // Arrange
        var pendingApprovalStatus = Status.Confirmed;
        var confirmedStatus = Status.Confirmed;

        // Act
        var pendingApprovalResult = pendingApprovalStatus.CanBeAuthorize();
        var confirmedResult = confirmedStatus.CanBeAuthorize();

        // Assert
        Assert.True(pendingApprovalResult);
        Assert.True(confirmedResult);
    }

    [Fact]
    public void
        IsCanAuthorizeShouldReturnFalseForAllStatusesExcludingPendingApprovalAndConfirmedStatuses()
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
        var confirmed = Status.Confirmed;
        var deactivated = Status.Deactivated;

        // Act
        var pendingConfirmationToConfirmedResult = pendingConfirmation.CanBeChangedToThisStatus(Status.Confirmed);
        var confirmedToDeactivatedResult = confirmed.CanBeChangedToThisStatus(Status.Deactivated);
        var deactivatedToDeletedResult = deactivated.CanBeChangedToThisStatus(Status.Deleted);
        var confirmedToDeletedResult = confirmed.CanBeChangedToThisStatus(Status.Deleted);
        var deactivatedToConfirmedResult = deactivated.CanBeChangedToThisStatus(Status.Confirmed);

        // Assert
        Assert.True(pendingConfirmationToConfirmedResult);
        Assert.True(confirmedToDeactivatedResult);
        Assert.True(deactivatedToDeletedResult);
        Assert.True(confirmedToDeletedResult);
        Assert.True(deactivatedToConfirmedResult);
    }

    [Fact]
    public void CanBeChangedToThisStatusWhenPotentialStatusIsInvalidForDomainStateMachineMustReturnFalse()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;
        var confirmed = Status.Confirmed;
        var deactivated = Status.Deactivated;

        // Act
        var confirmedToPendingConfirmationResult = confirmed.CanBeChangedToThisStatus(Status.PendingConfirmation);
        var pendingConfirmationToDeactivatedResult = pendingConfirmation.CanBeChangedToThisStatus(Status.Deactivated);
        var deactivatedToPendingConfirmationResult = deactivated.CanBeChangedToThisStatus(Status.PendingConfirmation);


        // Assert
        Assert.False(confirmedToPendingConfirmationResult);
        Assert.False(pendingConfirmationToDeactivatedResult);
        Assert.False(deactivatedToPendingConfirmationResult);
    }

    [Fact]
    public void PotentialStatusIsEqualCurrentStatusMustThrowAlreadyHaveThisStateException()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        void Act()
        {
            pendingConfirmation.CanBeChangedToThisStatus(Status.PendingConfirmation);
        }

        // Assert
        Assert.Throws<AlreadyHaveThisStateException>(Act);
    }

    [Fact]
    public void CanBeChangedToThisStatusWhenPotentialStatusIsNullMustThrowsValueIsRequiredException()
    {
        // Arrange
        var pendingConfirmation = Status.PendingConfirmation;

        // Act
        void Act()
        {
            pendingConfirmation.CanBeChangedToThisStatus(null);
        }

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
        var result = pendingConfirmation == Status.Confirmed;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NotEqOperatorForIdenticalStatusesMustReturnFalse()
    {
        // Arrange

        // Act
        var result = Status.PendingConfirmation != Status.PendingConfirmation;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NotEqOperatorForDifferentStatusesMustReturnTrue()
    {
        // Arrange

        // Act
        var result = Status.PendingConfirmation != Status.Confirmed;

        // Assert
        Assert.True(result);
    }
}