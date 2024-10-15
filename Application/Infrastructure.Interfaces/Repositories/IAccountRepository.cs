using Domain.AccountAggregate;

namespace Application.Infrastructure.Interfaces.Repositories;

public interface IAccountRepository
{ 
     Task<Account?> GetById(Guid id, CancellationToken cancellationToken = default);

     Task<Account?> GetByEmail(string email, CancellationToken cancellationToken = default);
     
     Task<(bool isExist, Guid confirmationId)> GetConfirmationRecord(Guid accountId);
     
     Task<bool> DeleteConfirmationRecord(Guid accountId);

     Task<bool> CreateAccount(Account account, Guid confirmationId);

     Task<bool> CreateRefreshToken(Guid accountId, string refreshToken, DateTime expiresIn);

     Task<bool> UpdateStatus(Account account);

     Task<bool> UpdatePassword(Account account);

     Task<bool> UpdateEmail(Account account);
}