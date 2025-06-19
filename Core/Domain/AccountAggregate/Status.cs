using Core.Domain.SharedKernel.Exceptions.InternalExceptions;
using Core.Domain.SharedKernel.Exceptions.PublicExceptions;
using CSharpFunctionalExtensions;

namespace Core.Domain.AccountAggregate;

public sealed class Status : Entity<int>
{
    public static readonly Status PendingConfirmation = new(1, nameof(PendingConfirmation).ToLowerInvariant());
    public static readonly Status Confirmed = new(2, nameof(Confirmed).ToLowerInvariant());
    public static readonly Status Deactivated = new(3, nameof(Deactivated).ToLowerInvariant());
    public static readonly Status Deleted = new(4, nameof(Deleted).ToLowerInvariant());

    private Status()
    {
    }

    private Status(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }


    public string Name { get; } = null!;

    public bool CanBeChangedToThisStatus(Status potentialStatus)
    {
        return potentialStatus switch
        {
            null => throw new ValueIsRequiredException($"{nameof(potentialStatus)} cannot be null"),
            _ when this == potentialStatus => throw new AlreadyHaveThisStateException(
                "account already have this status"),
            _ when this == PendingConfirmation && potentialStatus == Confirmed => true,
            _ when this == Confirmed && potentialStatus == Deactivated => true,
            _ when this == Deactivated && potentialStatus == Confirmed => true,
            _ when potentialStatus == Deleted => true,
            _ => false
        };
    }

    public bool CanBeAuthorize()
    {
        return this == Confirmed;
    }

    public static IEnumerable<Status> All()
    {
        return
        [
            Confirmed,
            PendingConfirmation,
            Deactivated,
            Deleted
        ];
    }

    public static Status FromName(string name)
    {
        var status = All()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (status == null) throw new ValueOutOfRangeException($"{nameof(name)} unknown status or null");
        return status;
    }

    public static Status FromId(int id)
    {
        var status = All().SingleOrDefault(s => s.Id == id);
        if (status == null) throw new ValueOutOfRangeException($"{nameof(id)} unknown status or null");
        return status;
    }

    public static bool operator ==(Status? a, Status? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Id == b.Id;
    }

    public static bool operator !=(Status a, Status b)
    {
        return !(a == b);
    }
}