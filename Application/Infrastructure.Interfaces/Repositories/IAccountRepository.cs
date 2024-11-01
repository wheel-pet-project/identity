using Domain.AccountAggregate;
using FluentResults;

namespace Application.Infrastructure.Interfaces.Repositories;

public interface IAccountRepository
{ 
     Task<Result<Account>> GetById(Guid id, CancellationToken cancellationToken = default);

     Task<Result<Account>> GetByEmail(string email, CancellationToken cancellationToken = default);
     
     Task<Result> AddAccountAndConfirmationToken(Account account, Guid confirmationToken);
     
     Task<Result> DeleteConfirmationToken(Guid accountId, Guid confirmationToken);

     Task<Result> AddRefreshTokenInfo(Guid refreshTokenId, Guid accountId);

     Task<Result<(Guid accountId, bool isRevoked)>> GetRefreshTokenInfo(Guid accountId);

     Task<Result> UpdateRefreshTokenInfo(Guid refreshTokenId, bool isRevoked);

     Task<Result> AddRefreshTokenInfoAndRevokeOldRefreshToken(Guid newRefreshTokenId, Guid accountId,
          Guid oldRefreshTokenId);
     
     Task<Result> UpdateStatus(Account account);

     Task<Result> UpdatePassword(Account account);
}