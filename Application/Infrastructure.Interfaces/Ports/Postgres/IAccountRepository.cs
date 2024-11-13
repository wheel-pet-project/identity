using Domain.AccountAggregate;
using FluentResults;

namespace Application.Infrastructure.Interfaces.Ports.Postgres;

public interface IAccountRepository
{ 
     Task<Result<Account>> GetById(Guid id, CancellationToken cancellationToken = default);

     Task<Result<Account>> GetByEmail(string Email, CancellationToken cancellationToken = default);
     
     Task<Result> UpdateStatus(Account account);
     
     Task<Result> UpdatePassword(Account account);
     
     Task<Result> AddAccountAndConfirmationToken(Account account, string confirmationTokenHash);

     Task<Result<string>> GetConfirmationToken(Guid accountId);
     
     Task<Result> DeleteConfirmationToken(Guid accountId, Guid confirmationToken);
}