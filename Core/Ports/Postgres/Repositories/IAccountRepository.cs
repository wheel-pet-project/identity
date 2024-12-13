using Core.Domain.AccountAggregate;

namespace Core.Ports.Postgres.Repositories;

public interface IAccountRepository
{ 
     Task<Account?> GetById(Guid id, CancellationToken cancellationToken = default);

     Task<Account?> GetByEmail(string email, CancellationToken cancellationToken = default);
     
     Task UpdateStatus(Account account);
     
     Task UpdatePasswordHash(Account account);
     
     Task Add(Account account);
}