using Ardalis.SmartEnum;

namespace Domain.AccountAggregate;

public class Role(string value, int id) : SmartEnum<Role>(value, id)
{
    public int Id { get; } = id;
    
    
    public static readonly Role Customer = new("Customer", 1);
    
    public static readonly Role Administrator = new("Administrator", 2);
    
    public static readonly Role Support = new("Support", 3);
    
    public static readonly Role Maintenance = new("Maintenance", 4);
    
    public static readonly Role HR = new("Hr", 5);
    
    
    public static bool operator == (Role a, Role b) => a.Value == b.Value;

    public static bool operator != (Role a, Role b) => a.Value != b.Value;
}