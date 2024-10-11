using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;

namespace Application.UseCases.Account.Create;

public class CreateAccountUseCase(
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher)
    : IUseCase<CreateAccountRequest, CreateAccountResponse>
{
    public async Task<CreateAccountResponse> Execute(CreateAccountRequest request)
    {
        var factory = new AccountFactory();
        var account = factory.CreateAccount( 
            request.Role, 
            Status.PendingConfirmation,
            request.Email, 
            request.Phone,
            passwordHasher.GenerateHash(request.Password));
        
        await accountRepository.Create(account, 
            confirmationId: Guid.NewGuid());
        
        // todo: add send message in notification service
        
        return new CreateAccountResponse(account.Id);
    }
}