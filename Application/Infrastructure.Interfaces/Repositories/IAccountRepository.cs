using Domain.AccountAggregate;

namespace Application.Infrastructure.Interfaces.Repositories;

public interface IAccountRepository
{ 
     Task<Account?> GetById(Guid id, CancellationToken cancellationToken = default);

     Task<Account?> GetByEmail(string email, CancellationToken cancellationToken = default);

     Task Create(Account account, Guid confirmationId);

     Task UpdateStatus(Account account);

     Task UpdatePassword(Account account);

     Task UpdateEmail(Account account);
}