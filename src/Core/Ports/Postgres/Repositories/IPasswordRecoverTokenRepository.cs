using Core.Domain.PasswordRecoverTokenAggregate;

namespace Core.Ports.Postgres.Repositories;

public interface IPasswordRecoverTokenRepository
{
    Task Add(PasswordRecoverToken token);

    Task<PasswordRecoverToken?> Get(Guid accountId);

    Task UpdateAppliedStatus(PasswordRecoverToken token);
}