using Proto.IdentityV1;
using DomainRole = Domain.AccountAggregate.Role;

namespace Api.Transformers;

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