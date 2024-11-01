using Domain.AccountAggregate;
using FluentResults;

namespace Application.Infrastructure.Interfaces.JwtProvider;

public interface IJwtProvider
{
    string GenerateAccessToken(Account account);

    string GenerateRefreshToken(Guid refreshTokenId);

    Task<Result<(Guid accountId, int roleId, int statusId)>> VerifyAccessToken(string accessToken);

    Task<Result<Guid>> VerifyRefreshToken(string refreshToken);
}