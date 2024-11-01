using Proto.Identity;
using DomainRole = Domain.AccountAggregate.Role;

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
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown role")
        };
    
    
    public Role ToResponse(int roleId) =>
        roleId switch
        {
            0 => Role.CustomerUnspecified,
            1 => Role.Admin,
            2 => Role.Support,
            3 => Role.Maintenance,
            4 => Role.Hr,
            _ => throw new ArgumentOutOfRangeException(nameof(roleId), roleId, "Unknown role")
        };


    public Role ToResponse(DomainRole r) =>
        r switch
        {
            DomainRole role when role == DomainRole.Customer => Role.CustomerUnspecified,
            DomainRole role when role == DomainRole.Admin => Role.Admin,
            DomainRole role when role == DomainRole.Support => Role.Support,
            DomainRole role when role == DomainRole.Maintenance => Role.Maintenance,
            DomainRole role when role == DomainRole.HR => Role.Hr,
            _ => throw new ArgumentOutOfRangeException(nameof(r), r, "Unknown role")
        };
}