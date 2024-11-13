using Ardalis.SmartEnum;

namespace Domain.AccountAggregate;

public class Role(string value, int id) : SmartEnum<Role>(value, id)
{
    public int Id { get; } = id;
    
    
    public static readonly Role Customer = new("Customer", 0);
    
    public static readonly Role Admin = new("Admin", 1);
    
    public static readonly Role Support = new("Support", 2);
    
    public static readonly Role Maintenance = new("Maintenance", 3);
    
    public static readonly Role HR = new("Hr", 4);


    public bool CanSetThisRole(Role newRole) =>
        newRole switch
        {
            null => throw new ArgumentNullException(nameof(newRole)),
            var role when role == this => false,
            _ when this == Customer => false,
            var role when this != Customer && role != Customer => true,
            _ => throw new ArgumentOutOfRangeException(nameof(newRole), newRole, "Unknown role")
        };

    public static Role FromId(int id) => FromValue(id);
    
    public static bool operator == (Role a, Role b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;
        
        return a.Id == b.Id;
    }

    public static bool operator != (Role a, Role b) => !(a == b);
}