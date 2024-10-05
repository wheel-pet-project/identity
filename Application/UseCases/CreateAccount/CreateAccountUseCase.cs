using Application.Infrastructure.Interfaces.EmailProvider;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Application.UseCases.Interfaces;
using Domain.AccountAggregate;

namespace Application.UseCases.CreateAccount;

public class CreateAccountUseCase(
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher,
    IEmailProvider emailProvider)
    : IUseCase<CreateAccountRequest, CreateAccountResponse>
{
    public async Task<CreateAccountResponse> Execute(CreateAccountRequest request)
    {
        var factory = new AccountFactory();
        var account = factory.CreateAccount(Guid.NewGuid(), 
            request.Role, 
            request.Email, 
            request.Phone,
            passwordHasher.GenerateHash(request.Password), 
            Status.PendingVerification);
        
        await accountRepository.Create(account);

        await emailProvider.SendVerificationEmail(account.Email, account.Id, Guid.NewGuid());
        
        return new CreateAccountResponse(request.CorrelationId, account.Id);
    }
}