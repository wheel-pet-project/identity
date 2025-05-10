using Core.Domain.AccountAggregate;
using Core.Infrastructure.Interfaces.JwtProvider;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authorize;

public class AuthorizeAccountHandler(IJwtProvider jwtProvider)
    : IRequestHandler<AuthorizeAccountCommand, Result<AuthorizeAccountResponse>>
{
    public async Task<Result<AuthorizeAccountResponse>> Handle(AuthorizeAccountCommand command, CancellationToken _)
    {
        var jwtVerifyingResult = await jwtProvider.VerifyJwtAccessToken(command.AccessToken);
        if (jwtVerifyingResult.IsFailed) return Result.Fail(jwtVerifyingResult.Errors);
        var accountAuthData = jwtVerifyingResult.Value;

        return MapToResponse(accountAuthData);
    }

    private AuthorizeAccountResponse MapToResponse((Guid accountId, Role role, Status status) accountAuthData)
    {
        return new AuthorizeAccountResponse(
            accountAuthData.accountId, accountAuthData.role, accountAuthData.status);
    }
}