using Core.Domain.RefreshTokenAggregate;
using FluentResults;

namespace Core.Ports.Postgres;

public interface IRefreshTokenRepository
{
    Task<Result> AddRefreshTokenInfo(RefreshToken refreshToken);

    Task<Result<RefreshToken>> GetRefreshTokenInfo(Guid refreshTokenId);

    Task<Result> AddRefreshTokenInfoAndRevokeOldRefreshToken(RefreshToken newRefreshToken, 
        RefreshToken oldRefreshToken);

    Task<Result> UpdateRefreshTokenInfo(Guid refreshTokenId, bool isRevoked);
}