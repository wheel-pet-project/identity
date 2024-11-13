using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.JwtProvider;
using Application.Infrastructure.Interfaces.Ports.Postgres;
using FluentResults;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.Application.UseCases.Account.RefreshAccessToken;

public class RefreshAccountAccessTokenUseCase(
    IAccountRepository accountRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider)
    : IUseCase<RefreshAccountAccessTokenRequest, RefreshAccountAccessTokenResponse>
{
    public async Task<Result<RefreshAccountAccessTokenResponse>> Execute(RefreshAccountAccessTokenRequest request)
    {
        var refreshTokenVerifyingResult = await jwtProvider.VerifyRefreshToken(request.RefreshToken);
        if (refreshTokenVerifyingResult.IsFailed) 
            return Result.Fail(refreshTokenVerifyingResult.Errors);
        
        var refreshTokenId = refreshTokenVerifyingResult.Value;
        var gettingTokenInfoResult = await refreshTokenRepository.GetRefreshTokenInfo(refreshTokenId);
        if (gettingTokenInfoResult.IsFailed)
            return Result.Fail(gettingTokenInfoResult.Errors);

        var tokenInfo = gettingTokenInfoResult.Value;
        if (tokenInfo.IsRevoked)
            return Result.Fail("Refresh token is revoked");
        
        var gettingAccountResult = await accountRepository.GetById(tokenInfo.AccountId);
        if (gettingAccountResult.IsFailed)
            throw new ApplicationException("Account not found", 
                "Refresh token not revoked for deleted account");

        var newRefreshTokenId = Guid.NewGuid();
        var account = gettingAccountResult.Value;
        var tokenUpdateResult = await refreshTokenRepository.AddRefreshTokenInfoAndRevokeOldRefreshToken(
            newRefreshTokenId, account.Id, refreshTokenId);
        
        var accessToken = jwtProvider.GenerateAccessToken(account);
        var newRefreshToken = jwtProvider.GenerateRefreshToken(newRefreshTokenId);
        
        return tokenUpdateResult.IsSuccess
            ? Result.Ok(new RefreshAccountAccessTokenResponse(accessToken, newRefreshToken))
            : Result.Fail(tokenUpdateResult.Errors);
    }
}