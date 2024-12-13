using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using CSharpFunctionalExtensions;

namespace Core.Domain.AccountAggregate;

public class Role : Entity<int>
{
    public static readonly Role Customer = new(1, nameof(Customer).ToLowerInvariant());
    public static readonly Role Admin = new(2, nameof(Admin).ToLowerInvariant());
    public static readonly Role Support = new(3, nameof(Support).ToLowerInvariant());
    public static readonly Role Maintenance = new(4, nameof(Maintenance).ToLowerInvariant());
    public static readonly Role Hr = new(5, nameof(Hr).ToLowerInvariant());
    
    private Role(){}

    private Role(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }
    
    public string Name { get; private set; }
    
    public bool CanBeChangedToThisRole(Role potentialRole) =>
        potentialRole switch
        {
            null => throw new ValueIsRequiredException("New role is null"),
            _ when potentialRole == this => false,
            _ when this == Customer => false,
            _ when this != Customer && potentialRole != Customer => true,
            _ => false
        };

    public static IEnumerable<Role> All()
    {
        yield return Customer;
        yield return Admin;
        yield return Support;
        yield return Maintenance;
        yield return Hr;
    }

    public static Role FromName(string name)
    {
        var role = All()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (role == null) throw new ValueOutOfRangeException("Unknown role name or null");
        return role;
    }

    public static Role FromId(int id)
    {
        var role = All().SingleOrDefault(r => r.Id == id);
        if (role == null) throw new ValueOutOfRangeException("Unknown role id or null");
        return role;
    }
    
    public static bool operator == (Role? a, Role? b)
    {
        if (a is null && b is null)
            return true;
        
        if (a is null || b is null)
            return false;
        
        return a.Id == b.Id;
    }

    public static bool operator != (Role? a, Role? b) => !(a == b);
}