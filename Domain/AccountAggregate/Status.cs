using Ardalis.SmartEnum;

namespace Domain.AccountAggregate;

public class Status(string value, int id) : SmartEnum<Status>(value, id)
{
    public int Id { get; } = id;
    
    public static readonly Status Unconfirmed = new Status("Unconfirmed", 1);
    
    public static readonly Status Confirmed = new Status("Confirmed", 2);
    
    public static readonly Status Deactivated = new Status("DeActive", 3);
    
    public static readonly Status Deleted = new Status("Deleted", 4);
    
    
    
    public static bool operator == (Status a, Status b) => a.Value == b.Value;
    
    public static bool operator != (Status a, Status b) => a.Value != b.Value;
}