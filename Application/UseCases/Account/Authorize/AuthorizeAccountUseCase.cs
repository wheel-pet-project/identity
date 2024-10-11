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
        var accData = await jwtProvider.VerifyToken(request.AccessToken);
        if (accData.isValid)
        {
            if(accData.status == Status.PendingConfirmation)
                throw new ApplicationException(
                    "Account is pending confirmation","" +
                    $"Account with id {accData.accId} is pending confirmation");
            return new AuthorizeAccountResponse(
                accData.accId, accData.role!, accData.status!);
        }
            
        
        throw new ApplicationException("Invalid token",
            $"Invalid token: {request.AccessToken}");
    }
}