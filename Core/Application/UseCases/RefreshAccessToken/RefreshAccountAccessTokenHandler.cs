using Core.Domain.RefreshTokenAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
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
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshAccountAccessTokenRequest, Result<RefreshAccountAccessTokenResponse>>
{
    public async Task<Result<RefreshAccountAccessTokenResponse>> Handle(RefreshAccountAccessTokenRequest request, 
        CancellationToken _)
    {
        var refreshTokenVerifyingResult = await jwtProvider.VerifyJwtRefreshToken(request.RefreshToken);
        if (refreshTokenVerifyingResult.IsFailed) return Result.Fail(refreshTokenVerifyingResult.Errors);
        var refreshTokenId = refreshTokenVerifyingResult.Value;

        var refreshToken = await refreshTokenRepository.GetNotRevokedToken(refreshTokenId);
        if (refreshToken is null) return Result.Fail(new NotFound("Refresh token not found"));

        if (!refreshToken.IsValid()) return Result.Fail("Refresh token is revoked or expired");

        var account = await accountRepository.GetById(refreshToken.AccountId);
        if (account is null) throw new DataConsistencyViolationException(
                "Data consistency violation: refresh token not revoked for deleted account");
        if (!account.Status.CanBeAuthorize()) return Result.Fail("Account cannot be authenticated");

        var newRefreshToken = RefreshToken.Create(account);

        await unitOfWork.BeginTransaction();
        await refreshTokenRepository.AddTokenAndRevokeOldToken(newRefreshToken, refreshToken);
        var transactionResult = await unitOfWork.Commit();

        return transactionResult.IsSuccess
            ? Result.Ok(new RefreshAccountAccessTokenResponse(jwtProvider.GenerateJwtAccessToken(account),
                jwtProvider.GenerateJwtRefreshToken(newRefreshToken.Id)))
            : Result.Fail(transactionResult.Errors);
    }
}