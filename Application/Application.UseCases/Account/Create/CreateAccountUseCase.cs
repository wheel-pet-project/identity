using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;
using FluentResults;

namespace Application.Application.UseCases.Account.Create;

public class CreateAccountUseCase(
    IAccountRepository accountRepository,
    IHasher hasher)
    : IUseCase<CreateAccountRequest, CreateAccountResponse>
{
    public async Task<Result<CreateAccountResponse>> Execute(CreateAccountRequest request)
    {
        var factory = new AccountFactory();
        var account = factory.CreateAccount( 
            request.Role, 
            Status.PendingConfirmation,
            request.Email, 
            request.Phone,
            hasher.GenerateHash(request.Password));
        
        var result = await accountRepository.AddAccountAndConfirmationToken(account, confirmationToken: Guid.NewGuid());

        if (result.IsFailed)
            return Result.Fail("Failed to create account");

        return Result.Ok(new CreateAccountResponse(account.Id));
    }
}