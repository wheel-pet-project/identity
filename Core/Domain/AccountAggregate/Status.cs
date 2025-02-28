using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using CSharpFunctionalExtensions;

namespace Core.Domain.AccountAggregate;

public sealed class Status : Entity<int>
{
    public static readonly Status Approved = new(1, nameof(Approved).ToLowerInvariant());
    public static readonly Status PendingConfirmation = new(2, nameof(PendingConfirmation).ToLowerInvariant());
    public static readonly Status PendingApproval = new(3, nameof(PendingApproval).ToLowerInvariant());
    public static readonly Status Deactivated = new(4, nameof(Deactivated).ToLowerInvariant());
    public static readonly Status Deleted = new(5, nameof(Deleted).ToLowerInvariant());

    private Status()
    {
    }

    private Status(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }


    public string Name { get; private set; } = null!;

    public bool CanBeChangedToThisStatus(Status potentialStatus)
    {
        return potentialStatus switch
        {
            null => throw new ValueIsRequiredException($"{nameof(potentialStatus)} cannot be null"),
            _ when this == potentialStatus => throw new DomainRulesViolationException(
                "account already has this status", isAlreadyInThisState: true),
            _ when this == PendingConfirmation && potentialStatus == PendingApproval => true,
            _ when this == PendingApproval && potentialStatus == Approved => true,
            _ when this == Approved && potentialStatus == Deactivated => true,
            _ when this == Approved && potentialStatus == PendingApproval => true,
            _ when this == Deactivated && potentialStatus == Approved => true,
            _ when potentialStatus == Deleted => true,
            _ => false
        };
    }

    public bool CanBeAuthorize()
    {
        return this == Approved || this == PendingApproval;
    }

    public static IEnumerable<Status> All()
    {
        return
        [
            Approved,
            PendingConfirmation,
            PendingApproval,
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