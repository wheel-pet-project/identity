using Proto.Identity;

namespace API.Transformers;

public class RoleTransformer
{
    public Domain.AccountAggregate.Role FromRequest(Role role) =>
        role switch
        {
            Role.CustomerUnspecified => Domain.AccountAggregate.Role.Customer,
            Role.Admin => Domain.AccountAggregate.Role.Admin,
            Role.Support => Domain.AccountAggregate.Role.Support,
            Role.Maintenance => Domain.AccountAggregate.Role.Maintenance,
            Role.Hr => Domain.AccountAggregate.Role.HR,
            _ => throw new ArgumentOutOfRangeException(nameof(role), 
                role, "Invalid role")
        };
    
    
    public Role ToResponse(int roleId) =>
        roleId switch
        {
            0 => Role.CustomerUnspecified,
            1 => Role.Admin,
            2 => Role.Support,
            3 => Role.Maintenance,
            4 => Role.Hr,
            _ => throw new ArgumentOutOfRangeException(nameof(roleId), 
                roleId, "Invalid role")
        };
    public Role ToResponse(Domain.AccountAggregate.Role role)
    {
        var result = Role.CustomerUnspecified;
        role.When(Domain.AccountAggregate.Role.Admin)
            .Then(() => result = Role.Admin);
        role.When(Domain.AccountAggregate.Role.Support)
            .Then(() => result = Role.Support);
        role.When(Domain.AccountAggregate.Role.Maintenance)
            .Then(() => result = Role.Maintenance);
        role.When(Domain.AccountAggregate.Role.HR)
            .Then(() => result = Role.Hr);
        
        return result;
    }
        
}