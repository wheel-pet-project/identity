using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.JwtProvider;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.UseCases.Account.Authenticate;

public class AuthenticateAccountUseCase(
    IAccountRepository accountRepository,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher) : IUseCase<AuthenticateAccountRequest, AuthenticateAccountResponse>
{
    public async Task<AuthenticateAccountResponse> Execute(AuthenticateAccountRequest request)
    {
        var account = await accountRepository.GetByEmail(request.Email);
        if (account == null)
            throw new ApplicationException(
                "Account not found", 
                $"Account with that email: {request.Email} not found");
        
        if(passwordHasher.VerifyHash(password: request.Password, 
               hash: account.Password) == false)
            throw new ApplicationException("Invalid password",
                $"Password: {account.Password} is not valid for account: {account.Id}");

        var accessToken = jwtProvider.GenerateToken(account);
        return new AuthenticateAccountResponse(accessToken);
    }
}