using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.JwtProvider;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Ports.Postgres;
using FluentResults;

namespace Application.Application.UseCases.Account.Authenticate;

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
        var result = await accountRepository.GetByEmail(request.Email);
        if (result.IsFailed)
            return Result.Fail(result.Errors);
        var account = result.Value;
        
        if (!account.Status.CanAuthorize())
            return Result.Fail("Account is not be authorized");
        if (!hasher.VerifyHash(request.Password, hash: account.Password))
            return Result.Fail("Password is incorrect");

        var accessToken = jwtProvider.GenerateAccessToken(account);
        var refreshTokenId = Guid.NewGuid();
        var refreshToken = jwtProvider.GenerateRefreshToken(refreshTokenId);

        var addingTokenInfoResult = await refreshTokenRepository.AddRefreshTokenInfo(refreshTokenId, account.Id);

        return addingTokenInfoResult.IsSuccess
            ? new AuthenticateAccountResponse(accessToken, refreshToken)
            : Result.Fail(addingTokenInfoResult.Errors);
    }
}