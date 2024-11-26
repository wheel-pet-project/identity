using Core.Domain.RefreshTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Ports.Postgres;
using FluentResults;

namespace Core.Application.UseCases.RefreshAccessToken;

public class RefreshAccountAccessTokenUseCase(
    IAccountRepository accountRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider)
    : IUseCase<RefreshAccountAccessTokenRequest, RefreshAccountAccessTokenResponse>
{
    public async Task<Result<RefreshAccountAccessTokenResponse>> Execute(RefreshAccountAccessTokenRequest request)
    {
        var refreshTokenVerifyingResult = await jwtProvider.VerifyJwtRefreshToken(request.RefreshToken);
        if (refreshTokenVerifyingResult.IsFailed) return Result.Fail(refreshTokenVerifyingResult.Errors);
        
        var refreshTokenId = refreshTokenVerifyingResult.Value;
        var gettingTokenInfoResult = await refreshTokenRepository.GetRefreshTokenInfo(refreshTokenId);
        if (gettingTokenInfoResult.IsFailed) return Result.Fail(gettingTokenInfoResult.Errors);
        var oldRefreshToken = gettingTokenInfoResult.Value;
        
        if (!oldRefreshToken.IsValid()) return Result.Fail("Refresh token is revoked or expired");
        
        var gettingAccountResult = await accountRepository.GetById(oldRefreshToken.AccountId);
        if (gettingAccountResult.IsFailed) throw new DataConsistencyViolationException(
                "Data consistency violation: refresh token not revoked for deleted account");
        var account = gettingAccountResult.Value;

        var newRefreshToken = RefreshToken.Create(account);
        var tokenUpdateResult = await refreshTokenRepository.AddRefreshTokenInfoAndRevokeOldRefreshToken(
            newRefreshToken, oldRefreshToken);
        
        var accessToken = jwtProvider.GenerateJwtAccessToken(account);
        var jwtRefreshToken = jwtProvider.GenerateJwtRefreshToken(newRefreshToken.Id);
        
        return tokenUpdateResult.IsSuccess
            ? Result.Ok(new RefreshAccountAccessTokenResponse(accessToken, jwtRefreshToken))
            : Result.Fail(tokenUpdateResult.Errors);
    }
}