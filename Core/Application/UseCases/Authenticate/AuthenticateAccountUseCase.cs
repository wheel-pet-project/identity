using Core.Domain.RefreshTokenAggregate;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using FluentResults;

namespace Core.Application.UseCases.Authenticate;

public class AuthenticateAccountUseCase(
    IAccountRepository accountRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider,
    IHasher hasher) 
    : IUseCase<AuthenticateAccountRequest, AuthenticateAccountResponse>
{
    public async Task<Result<AuthenticateAccountResponse>> Execute(
        AuthenticateAccountRequest request)
    {
        var gettingAccountByEmailResult = await accountRepository.GetByEmail(request.Email);
        if (gettingAccountByEmailResult.IsFailed) return Result.Fail(gettingAccountByEmailResult.Errors);
        var account = gettingAccountByEmailResult.Value;
        
        if (!account.Status.CanBeAuthorize()) return Result.Fail("Account is not be authorized");
        if (!hasher.VerifyHash(request.Password, hash: account.PasswordHash))
            return Result.Fail("Password is incorrect");

        var accessToken = jwtProvider.GenerateJwtAccessToken(account);
        var refreshToken = RefreshToken.Create(account);
        var jwtRefreshToken = jwtProvider.GenerateJwtRefreshToken(refreshToken.Id);

        var addingTokenInfoResult = await refreshTokenRepository.AddRefreshTokenInfo(refreshToken);

        return addingTokenInfoResult.IsSuccess
            ? new AuthenticateAccountResponse(accessToken, jwtRefreshToken)
            : Result.Fail(addingTokenInfoResult.Errors);
    }
}