using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Domain.SharedKernel.Exceptions.InternalExceptions;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RefreshAccessToken;

public class RefreshAccountAccessTokenHandler(
    IAccountRepository accountRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshAccountAccessTokenCommand, Result<RefreshAccountAccessTokenResponse>>
{
    public async Task<Result<RefreshAccountAccessTokenResponse>> Handle(
        RefreshAccountAccessTokenCommand command,
        CancellationToken _)
    {
        var verifyingAndGettingTokenResult = await VerifyJwtAndGetRefreshTokenAggregate(command);
        if (verifyingAndGettingTokenResult.IsFailed) return Result.Fail(verifyingAndGettingTokenResult.Errors);
        var refreshToken = verifyingAndGettingTokenResult.Value;
        if (!refreshToken.IsValid(timeProvider)) return Result.Fail("Refresh token is revoked or expired");

        var account = await accountRepository.GetById(refreshToken.AccountId, _);
        if (account is null)
            throw new DataConsistencyViolationException(
                "Data consistency violation: refresh token not revoked for deleted account");
        if (!account.Status.CanBeAuthorize()) return Result.Fail("Account cannot be authenticated");

        var (newRefreshToken, newJwtAccessToken, newJwtRefreshToken) =
            CreateNewRefreshTokenAndJwtTokens(account, refreshToken);

        var transactionResult = await SaveInTransaction(async () =>
            await refreshTokenRepository.AddTokenAndRevokeOldToken(newRefreshToken, refreshToken));

        return transactionResult.IsSuccess
            ? Result.Ok(MapToResponse(newJwtAccessToken, newJwtRefreshToken))
            : Result.Fail(transactionResult.Errors);
    }

    private async Task<Result<RefreshToken>> VerifyJwtAndGetRefreshTokenAggregate(
        RefreshAccountAccessTokenCommand command)
    {
        var refreshTokenVerifyingResult = await jwtProvider.VerifyJwtRefreshToken(command.RefreshToken);
        if (refreshTokenVerifyingResult.IsFailed) return Result.Fail(refreshTokenVerifyingResult.Errors);
        var refreshTokenId = refreshTokenVerifyingResult.Value;

        var refreshToken = await refreshTokenRepository.GetNotRevokedToken(refreshTokenId);
        if (refreshToken is null) return Result.Fail(new NotFound("Refresh token not found"));

        return refreshToken;
    }

    private (RefreshToken newRefreshToken, string newJwtAccessToken, string newJwtRefreshToken)
        CreateNewRefreshTokenAndJwtTokens(Account account, RefreshToken refreshToken)
    {
        var newRefreshToken = RefreshToken.Create(account, timeProvider);
        var newJwtAccessToken = jwtProvider.GenerateJwtAccessToken(account);
        var newJwtRefreshToken = jwtProvider.GenerateJwtRefreshToken(refreshToken);

        return (newRefreshToken, newJwtAccessToken, newJwtRefreshToken);
    }

    private async Task<Result> SaveInTransaction(Func<Task> execute)
    {
        await unitOfWork.BeginTransaction();

        await execute();

        return await unitOfWork.Commit();
    }

    private RefreshAccountAccessTokenResponse MapToResponse(string jwtAccessToken, string jwtRefreshToken)
    {
        return new RefreshAccountAccessTokenResponse(jwtAccessToken, jwtRefreshToken);
    }
}