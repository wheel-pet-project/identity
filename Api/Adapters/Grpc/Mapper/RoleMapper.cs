using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Proto.IdentityV1;
using DomainRole = Core.Domain.AccountAggregate.Role;

namespace Api.Adapters.Grpc.Mapper;

public class RoleMapper
{
    public DomainRole RoleFromRequest(Role protoRole)
    {
        return protoRole switch
        {
            Role.CustomerUnspecified => DomainRole.Customer,
            Role.Admin => DomainRole.Admin,
            Role.Support => DomainRole.Support,
            Role.Maintenance => DomainRole.Maintenance,
            Role.Hr => DomainRole.Hr,
            _ => throw new ValueOutOfRangeException($"{nameof(protoRole)} is unknown role")
        };
    }

    public Role RoleToResponse(DomainRole domainRole)
    {
        return domainRole switch
        {
            _ when domainRole == DomainRole.Customer => Role.CustomerUnspecified,
            _ when domainRole == DomainRole.Admin => Role.Admin,
            _ when domainRole == DomainRole.Support => Role.Support,
            _ when domainRole == DomainRole.Maintenance => Role.Maintenance,
            _ when domainRole == DomainRole.Hr => Role.Hr,
            _ => throw new ValueOutOfRangeException($"{nameof(domainRole)} is unknown role")
        };
    }
}