using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
using FluentResults;

namespace Core.Infrastructure.Interfaces.JwtProvider;

public interface IJwtProvider
{
    string GenerateJwtAccessToken(Account account);

    string GenerateJwtRefreshToken(RefreshToken refreshToken);

    Task<Result<(Guid accountId, Role role, Status status)>> VerifyJwtAccessToken(string accessToken);

    Task<Result<Guid>> VerifyJwtRefreshToken(string refreshToken);
}