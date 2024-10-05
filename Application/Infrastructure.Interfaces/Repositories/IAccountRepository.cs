using Domain.AccountAggregate;

namespace Application.Infrastructure.Interfaces.Repositories;

public interface IAccountRepository
{ 
     Task<Account?> GetById(Guid id);

     Task<Account?> GetByEmail(string email);

     Task Create(Account account);

     Task UpdateStatus(Account account);

     Task UpdatePassword(Account account);

     Task UpdateEmail(Account account);
}