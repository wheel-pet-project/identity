using Core.Domain.RefreshTokenAggregate;

namespace Core.Ports.Postgres.Repositories;

public interface IRefreshTokenRepository
{
    Task Add(RefreshToken refreshToken);

    Task<RefreshToken?> GetNotRevokedToken(Guid refreshTokenId);
    
    Task<List<RefreshToken>> GetNotRevokedTokensByAccountId(Guid accountId);

    Task AddTokenAndRevokeOldToken(RefreshToken newRefreshToken, 
        RefreshToken oldRefreshToken);

    Task UpdateRevokeStatus(RefreshToken refreshToken);
}