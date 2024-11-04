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

        var confirmationToken = Guid.NewGuid();
        Console.BackgroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Confirmation token: {confirmationToken}");
        var result = await accountRepository.AddAccountAndConfirmationToken(account, 
            confirmationTokenHash: hasher.GenerateHash(confirmationToken.ToString()));
        
        // send command to notification

        return result.IsSuccess 
            ? Result.Ok(new CreateAccountResponse(account.Id)) 
            : Result.Fail(result.Errors);
    }
}