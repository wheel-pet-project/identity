using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.JwtProvider;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.UseCases.Account.Authenticate;

public class AuthenticateAccountUseCase(
    IAccountRepository accountRepository,
    IJwtProvider jwtProvider,
    IHasher hasher) 
    : IUseCase<AuthenticateAccountRequest, AuthenticateAccountResponse>
{
    public async Task<AuthenticateAccountResponse> Execute(
        AuthenticateAccountRequest request)
    {
        var account = await accountRepository.GetByEmail(request.Email);
        if (account == null)
            throw new ApplicationException(
                "Account not found", 
                $"Account with that email: {request.Email} not found");
        
        if(account.Status == Status.PendingConfirmation)
            throw new ApplicationException("Account is pending confirmation",
                $"Account with id {account.Id} is pending confirmation");
        
        if(hasher.VerifyHash(request.Password, 
               hash: account.Password) == false)
            throw new ApplicationException("Invalid password",
                $"Password: {account.Password} is not valid for account: {account.Id}");

        var accessToken = jwtProvider.GenerateToken(account);
        var refreshToken = Guid.NewGuid().ToString();
        var refreshHash = hasher.GenerateHash(refreshToken);

        await accountRepository.CreateRefreshToken(account.Id, refreshHash, DateTime.UtcNow.AddDays(60));
        
        return new AuthenticateAccountResponse(accessToken, refreshToken);
    }
}