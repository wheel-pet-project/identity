using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.UseCases.Account.Create;

public class CreateAccountUseCase(
    IAccountRepository accountRepository,
    IHasher hasher)
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
            hasher.GenerateHash(request.Password));
        
        var isSuccess = await accountRepository.CreateAccount(account, 
            confirmationId: Guid.NewGuid());
        if (isSuccess)
        {
            // todo: send message to notification service
            return new CreateAccountResponse(account.Id);
        }

        throw new ApplicationException("Create account failed, this email already used", 
            $"Could not create account with id {account.Id}");
    }
}