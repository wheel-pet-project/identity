using Domain.AccountAggregate;
using FluentResults;

namespace Application.Infrastructure.Interfaces.JwtProvider;

public interface IJwtProvider
{
    string GenerateAccessToken(Account account);

    string GenerateRefreshToken(Guid refreshTokenId);

    Task<Result<(Guid accountId, Role role, Status status)>> VerifyAccessToken(string accessToken);

    Task<Result<Guid>> VerifyRefreshToken(string refreshToken);
}