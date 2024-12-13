using Proto.IdentityV1;
using DomainRole = Core.Domain.AccountAggregate.Role;
using DomainStatus = Core.Domain.AccountAggregate.Status;

namespace Api.Mapper;

public class Mapper
{
    public DomainRole RoleFromRequest(Role role)
    {
        var roleMapper = new RoleMapper();
        return roleMapper.RoleFromRequest(role);
    }

    public Role RoleToResponse(DomainRole role)
    {
        var roleMapper = new RoleMapper();
        return roleMapper.RoleToResponse(role);
    }


    public Status StatusToResponse(DomainStatus status)
    {
        var statusMapper = new StatusMapper();
        return statusMapper.StatusToResponse(status);
    }
}