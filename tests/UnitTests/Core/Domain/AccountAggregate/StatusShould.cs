using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.InternalExceptions;
using Core.Domain.SharedKernel.Exceptions.PublicExceptions;
using Xunit;

namespace Core.Tests.Domain.AccountAggregate;

public class StatusTests
{
    [Fact]
    public void Throw_exception_when_potential_status_is_null()
    {
        // Arrange
        var sut = Status.Confirmed;

        // Act & Assert
        var exception = Assert.Throws<ValueIsRequiredException>(() => sut.CanBeChangedToThisStatus(null));
        Assert.Contains("potentialStatus", exception.Message);
    }

    [Fact]
    public void Throw_exception_when_potential_status_is_same()
    {
        // Arrange
        var sut = Status.Confirmed;

        // Act & Assert
        var exception = Assert.Throws<AlreadyHaveThisStateException>(() => sut.CanBeChangedToThisStatus(Status.Confirmed));
        Assert.Contains("account already have this status", exception.Message);
    }

    [Fact]
    public void Return_true_when_pending_confirmation_to_confirmed()
    {
        // Arrange
        var sut = Status.PendingConfirmation;

        // Act
        var result = sut.CanBeChangedToThisStatus(Status.Confirmed);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Return_true_when_confirmed_to_deactivated()
    {
        // Arrange
        var sut = Status.Confirmed;

        // Act
        var result = sut.CanBeChangedToThisStatus(Status.Deactivated);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Return_true_when_deactivated_to_confirmed()
    {
        // Arrange
        var sut = Status.Deactivated;

        // Act
        var result = sut.CanBeChangedToThisStatus(Status.Confirmed);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Return_true_when_any_status_to_deleted()
    {
        // Arrange & Act
        var pendingToDeleted = Status.PendingConfirmation.CanBeChangedToThisStatus(Status.Deleted);
        var confirmedToDeleted = Status.Confirmed.CanBeChangedToThisStatus(Status.Deleted);
        var deactivatedToDeleted = Status.Deactivated.CanBeChangedToThisStatus(Status.Deleted);

        // Assert
        Assert.True(pendingToDeleted);
        Assert.True(confirmedToDeleted);
        Assert.True(deactivatedToDeleted);
    }

    [Fact]
    public void Return_false_when_confirmed_to_pending_confirmation()
    {
        // Arrange
        var sut = Status.Confirmed;

        // Act
        var result = sut.CanBeChangedToThisStatus(Status.PendingConfirmation);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Return_false_when_pending_confirmation_to_deactivated()
    {
        // Arrange
        var sut = Status.PendingConfirmation;

        // Act
        var result = sut.CanBeChangedToThisStatus(Status.Deactivated);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Return_false_when_deleted_to_any_status()
    {
        // Arrange
        var sut = Status.Deleted;

        // Act
        var deletedToPending = sut.CanBeChangedToThisStatus(Status.PendingConfirmation);
        var deletedToConfirmed = sut.CanBeChangedToThisStatus(Status.Confirmed);
        var deletedToDeactivated = sut.CanBeChangedToThisStatus(Status.Deactivated);

        // Assert
        Assert.False(deletedToPending);
        Assert.False(deletedToConfirmed);
        Assert.False(deletedToDeactivated);
    }

    [Fact]
    public void Return_true_when_confirmed_status_can_authorize()
    {
        // Arrange
        var sut = Status.Confirmed;

        // Act
        var result = sut.CanBeAuthorize();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Return_false_when_pending_confirmation_status_cannot_authorize()
    {
        // Arrange
        var sut = Status.PendingConfirmation;

        // Act
        var result = sut.CanBeAuthorize();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Return_false_when_deactivated_status_cannot_authorize()
    {
        // Arrange
        var sut = Status.Deactivated;

        // Act
        var result = sut.CanBeAuthorize();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Return_false_when_deleted_status_cannot_authorize()
    {
        // Arrange
        var sut = Status.Deleted;

        // Act
        var result = sut.CanBeAuthorize();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Return_all_statuses()
    {
        // Act
        var result = Status.All().ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains(Status.Confirmed, result);
        Assert.Contains(Status.PendingConfirmation, result);
        Assert.Contains(Status.Deactivated, result);
        Assert.Contains(Status.Deleted, result);
    }

    [Theory]
    [InlineData("confirmed")]
    [InlineData("CONFIRMED")]
    [InlineData("Confirmed")]
    public void Return_status_when_name_exists_ignoring_case(string name)
    {
        // Act
        var result = Status.FromName(name);

        // Assert
        Assert.Equal(Status.Confirmed, result);
    }

    [Fact]
    public void Return_status_when_exact_name_exists()
    {
        // Act
        var result = Status.FromName("pendingconfirmation");

        // Assert
        Assert.Equal(Status.PendingConfirmation, result);
    }

    [Fact]
    public void Throw_exception_when_name_not_exists()
    {
        // Act & Assert
        var exception = Assert.Throws<ValueOutOfRangeException>(() => Status.FromName("unknown"));
        Assert.Contains("name", exception.Message);
    }

    [Fact]
    public void Throw_exception_when_name_is_null()
    {
        // Act & Assert
        var exception = Assert.Throws<ValueOutOfRangeException>(() => Status.FromName(null));
        Assert.Contains("name", exception.Message);
    }

    [Fact]
    public void Return_status_when_id_exists()
    {
        // Act
        var result = Status.FromId(1);

        // Assert
        Assert.Equal(Status.PendingConfirmation, result);
    }

    [Fact]
    public void Throw_exception_when_id_not_exists()
    {
        // Act & Assert
        var exception = Assert.Throws<ValueOutOfRangeException>(() => Status.FromId(999));
        Assert.Contains("id", exception.Message);
    }

    [Fact]
    public void Return_true_when_same_statuses_are_equal()
    {
        // Arrange
        var status1 = Status.Confirmed;
        var status2 = Status.Confirmed;

        // Act
        var result = status1 == status2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Return_false_when_different_statuses_are_not_equal()
    {
        // Arrange
        var status1 = Status.Confirmed;
        var status2 = Status.PendingConfirmation;

        // Act
        var result = status1 == status2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Return_true_when_both_statuses_are_null()
    {
        // Arrange
        Status status1 = null;
        Status status2 = null;

        // Act
        var result = status1 == status2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Return_false_when_one_status_is_null()
    {
        // Arrange
        var status1 = Status.Confirmed;
        Status status2 = null;

        // Act
        var result1 = status1 == status2;
        var result2 = status2 == status1;

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Return_false_when_same_statuses_are_not_not_equal()
    {
        // Arrange
        var status1 = Status.Confirmed;
        var status2 = Status.Confirmed;

        // Act
        var result = status1 != status2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Return_true_when_different_statuses_are_not_equal()
    {
        // Arrange
        var status1 = Status.Confirmed;
        var status2 = Status.PendingConfirmation;

        // Act
        var result = status1 != status2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Static_fields_have_correct_ids_and_names()
    {
        // Assert
        Assert.Equal(1, Status.PendingConfirmation.Id);
        Assert.Equal("pendingconfirmation", Status.PendingConfirmation.Name);
        
        Assert.Equal(2, Status.Confirmed.Id);
        Assert.Equal("confirmed", Status.Confirmed.Name);
        
        Assert.Equal(3, Status.Deactivated.Id);
        Assert.Equal("deactivated", Status.Deactivated.Name);
        
        Assert.Equal(4, Status.Deleted.Id);
        Assert.Equal("deleted", Status.Deleted.Name);
    }
}