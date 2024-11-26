using Proto.IdentityV1;
using DomainRole = Core.Domain.AccountAggregate.Role;

namespace Api.Transformers;

public class RoleTransformer
{
    public DomainRole FromRequest(Role role) =>
        role switch
        {
            Role.CustomerUnspecified => DomainRole.Customer,
            Role.Admin => DomainRole.Admin,
            Role.Support => DomainRole.Support,
            Role.Maintenance => DomainRole.Maintenance,
            Role.Hr => DomainRole.Hr,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown role")
        };

    public Role ToResponse(DomainRole r) =>
        r switch
        {
            var role when role == DomainRole.Customer => Role.CustomerUnspecified,
            var role when role == DomainRole.Admin => Role.Admin,
            var role when role == DomainRole.Support => Role.Support,
            var role when role == DomainRole.Maintenance => Role.Maintenance,
            var role when role == DomainRole.Hr => Role.Hr,
            _ => throw new ArgumentOutOfRangeException(nameof(r), r, "Unknown role")
        };
}