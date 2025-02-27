using Core.Infrastructure.Interfaces.JwtProvider;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authorize;

public class AuthorizeAccountHandler(IJwtProvider jwtProvider)
    : IRequestHandler<AuthorizeAccountRequest, Result<AuthorizeAccountResponse>>
{
    public async Task<Result<AuthorizeAccountResponse>> Handle(AuthorizeAccountRequest request, CancellationToken _)
    {
        var result = await jwtProvider.VerifyJwtAccessToken(request.AccessToken);
        if (result.IsFailed) return Result.Fail(result.Errors);
        var accountAuthData = result.Value;

        return Result.Ok(new AuthorizeAccountResponse(
            accountAuthData.accountId, accountAuthData.role, accountAuthData.status));
    }
}