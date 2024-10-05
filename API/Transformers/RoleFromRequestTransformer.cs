namespace API.Transformers;

public class RoleFromRequestTransformer
{
    public Domain.AccountAggregate.Role FromRequest(Role role) =>
        role switch
        {
            Role.Customer => Domain.AccountAggregate.Role.Customer,
            Role.Admin => Domain.AccountAggregate.Role.Admin,
            Role.Support => Domain.AccountAggregate.Role.Support,
            Role.Maintenance => Domain.AccountAggregate.Role.Maintenance,
            Role.Hr => Domain.AccountAggregate.Role.HR,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid role")
        };
}