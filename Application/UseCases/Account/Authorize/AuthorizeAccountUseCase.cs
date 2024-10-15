using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.JwtProvider;
using Domain.AccountAggregate;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.UseCases.Account.Authorize;

public class AuthorizeAccountUseCase(IJwtProvider jwtProvider) 
    : IUseCase<AuthorizeAccountRequest, AuthorizeAccountResponse>
{
    public async Task<AuthorizeAccountResponse> Execute(AuthorizeAccountRequest request)
    {
        var verificationResult = await jwtProvider.VerifyToken(request.AccessToken);
        if (verificationResult.isValid)
            return new AuthorizeAccountResponse(
                verificationResult.accId, verificationResult.role, verificationResult.status);
        
        throw new ApplicationException("Invalid token",
            $"Invalid token: {request.AccessToken}");
    }
}