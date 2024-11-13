using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.JwtProvider;
using FluentResults;

namespace Application.Application.UseCases.Account.Authorize;

public class AuthorizeAccountUseCase(IJwtProvider jwtProvider) 
    : IUseCase<AuthorizeAccountRequest, AuthorizeAccountResponse>
{
    public async Task<Result<AuthorizeAccountResponse>> Execute(AuthorizeAccountRequest request)
    {
        var result = await jwtProvider.VerifyAccessToken(request.AccessToken);
        if (result.IsFailed)
            return Result.Fail(result.Errors);
        var accountAuthData = result.Value;
        
        return Result.Ok(new AuthorizeAccountResponse(
                accountAuthData.accountId, 
                accountAuthData.role, 
                accountAuthData.status));
    }
}