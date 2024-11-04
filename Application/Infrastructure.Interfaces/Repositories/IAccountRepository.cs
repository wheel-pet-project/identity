using Domain.AccountAggregate;
using FluentResults;

namespace Application.Infrastructure.Interfaces.Repositories;

public interface IAccountRepository
{ 
     Task<Result<Account>> GetById(Guid id, CancellationToken cancellationToken = default);

     Task<Result<Account>> GetByEmail(string Email, CancellationToken cancellationToken = default);
     
     Task<Result> AddAccountAndConfirmationToken(Account account, string confirmationTokenHash);

     Task<Result<string>> GetConfirmationToken(Guid accountId);
     
     Task<Result> DeleteConfirmationToken(Guid accountId, Guid confirmationToken);

     Task<Result> AddRefreshTokenInfo(Guid refreshTokenId, Guid accountId);

     Task<Result<(Guid AccountId, bool IsRevoked)>> GetRefreshTokenInfo(Guid accountId);

     Task<Result> UpdateRefreshTokenInfo(Guid refreshTokenId, bool isRevoked);

     Task<Result> AddRefreshTokenInfoAndRevokeOldRefreshToken(Guid newRefreshTokenId, Guid accountId,
          Guid oldRefreshTokenId);
     
     Task<Result> UpdateStatus(Account account);

     Task<Result> UpdatePassword(Account account);
}