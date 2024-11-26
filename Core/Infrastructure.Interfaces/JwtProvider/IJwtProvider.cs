using Core.Domain.AccountAggregate;
using FluentResults;

namespace Core.Infrastructure.Interfaces.JwtProvider;

public interface IJwtProvider
{
    string GenerateJwtAccessToken(Account account);

    string GenerateJwtRefreshToken(Guid refreshTokenId);

    Task<Result<(Guid accountId, Role role, Status status)>> VerifyJwtAccessToken(string accessToken);

    Task<Result<Guid>> VerifyJwtRefreshToken(string refreshToken);
}