using Core.Infrastructure.Interfaces.JwtProvider;
using FluentResults;

namespace Core.Application.UseCases.Authorize;

public class AuthorizeAccountUseCase(IJwtProvider jwtProvider) 
    : IUseCase<AuthorizeAccountRequest, AuthorizeAccountResponse>
{
    public async Task<Result<AuthorizeAccountResponse>> Execute(AuthorizeAccountRequest request)
    {
        var result = await jwtProvider.VerifyJwtAccessToken(request.AccessToken);
        if (result.IsFailed) return Result.Fail(result.Errors);
        var accountAuthData = result.Value;
        
        return Result.Ok(new AuthorizeAccountResponse(
            accountAuthData.accountId, accountAuthData.role, accountAuthData.status));
    }
}