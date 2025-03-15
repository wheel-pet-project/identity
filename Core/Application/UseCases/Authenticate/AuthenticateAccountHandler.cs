using Core.Domain.RefreshTokenAggregate;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authenticate;

public class AuthenticateAccountHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAccountRepository accountRepository,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    IHasher hasher,
    TimeProvider timeProvider)
    : IRequestHandler<AuthenticateAccountCommand, Result<AuthenticateAccountResponse>>
{
    public async Task<Result<AuthenticateAccountResponse>> Handle(
        AuthenticateAccountCommand command,
        CancellationToken _)
    {
        var account = await accountRepository.GetByEmail(command.Email);
        if (account is null) return Result.Fail("Account does not found");

        if (!account.Status.CanBeAuthorize()) return Result.Fail("Account cannot be authenticated");
        if (!hasher.VerifyHash(command.Password, account.PasswordHash)) return Result.Fail("Password is incorrect");

        var refreshToken = RefreshToken.Create(account, timeProvider);
        var jwtAccessToken = jwtProvider.GenerateJwtAccessToken(account);
        var jwtRefreshToken = jwtProvider.GenerateJwtRefreshToken(refreshToken.Id);

        await unitOfWork.BeginTransaction();
        await refreshTokenRepository.Add(refreshToken);
        var transactionResult = await unitOfWork.Commit();

        return transactionResult.IsSuccess
            ? new AuthenticateAccountResponse(jwtAccessToken, jwtRefreshToken)
            : transactionResult;
    }
}