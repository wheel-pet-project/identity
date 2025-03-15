using Core.Infrastructure.Interfaces.JwtProvider;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authorize;

public class AuthorizeAccountHandler(IJwtProvider jwtProvider)
    : IRequestHandler<AuthorizeAccountCommand, Result<AuthorizeAccountResponse>>
{
    public async Task<Result<AuthorizeAccountResponse>> Handle(AuthorizeAccountCommand command, CancellationToken _)
    {
        var result = await jwtProvider.VerifyJwtAccessToken(command.AccessToken);
        if (result.IsFailed) return Result.Fail(result.Errors);
        var accountAuthData = result.Value;

        return Result.Ok(new AuthorizeAccountResponse(
            accountAuthData.accountId, accountAuthData.role, accountAuthData.status));
    }
}