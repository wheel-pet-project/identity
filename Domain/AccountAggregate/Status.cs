using Ardalis.SmartEnum;

namespace Domain.AccountAggregate;

public class Status(string value, int id) : SmartEnum<Status>(value, id)
{
    public int Id { get; } = id;
    
    public static readonly Status Approved = new Status("Approved", 0);
    
    public static readonly Status PendingConfirmation = new Status("Pending confirmation", 1);
    
    public static readonly Status PendingApproval = new Status("Pending approval", 2);
    
    public static readonly Status Deactivated = new Status("Deactivated", 3);
    
    public static readonly Status Deleted = new Status("Deleted", 4);


    public bool CanSetThisStatus(Status newStatus) =>
        newStatus switch
        {
            null => throw new ArgumentNullException(nameof(newStatus)),
            var status when status == this => false,
            var status when this != Deactivated && status == Deactivated => true,
            var status when this != Deleted && status == Deleted => true,
            var status when this == PendingConfirmation && status == PendingApproval => true,
            var status when this == PendingApproval && status == Approved => true,
            _ => throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, "Unknown status")  
        };

    public static Status FromId(int id) => FromValue(id);

    public bool CanAuthorize() => this == Approved || this == PendingApproval;

    public static bool operator == (Status? a, Status? b)
    {
        if (a is null && b is null)
            return true;
        
        if (a is null || b is null)
            return false;
        
        return a.Id == b.Id;
    }

    public static bool operator != (Status a, Status b) => !(a == b);
}