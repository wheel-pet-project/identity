using Core.Domain.ConfirmationTokenAggregate;

namespace Core.Ports.Postgres.Repositories;

public interface IConfirmationTokenRepository
{
    Task Add(ConfirmationToken confirmationToken);

    Task<ConfirmationToken?> Get(Guid accountId);

    Task Delete(Guid accountId);
}