using FluentResults;

namespace Application.Infrastructure.Interfaces.Ports.Postgres;

public interface IRefreshTokenRepository
{
    Task<Result> AddRefreshTokenInfo(Guid refreshTokenId, Guid accountId);

    Task<Result<(Guid AccountId, bool IsRevoked)>> GetRefreshTokenInfo(Guid refreshTokenId);

    Task<Result> AddRefreshTokenInfoAndRevokeOldRefreshToken(
        Guid newRefreshTokenId, Guid accountId, Guid oldRefreshTokenId);

    Task<Result> UpdateRefreshTokenInfo(Guid refreshTokenId, bool isRevoked);
}