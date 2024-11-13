using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Ports.Postgres;
using FluentResults;

namespace Application.Application.UseCases.Account.Create;

public class CreateAccountUseCase(
    IAccountRepository accountRepository,
    IHasher hasher)
    : IUseCase<CreateAccountRequest, CreateAccountResponse>
{
    public async Task<Result<CreateAccountResponse>> Execute(CreateAccountRequest request)
    {
        var account = new Domain.AccountAggregate.Account( 
            request.Role, 
            request.Email, 
            request.Phone,
            hasher.GenerateHash(request.Password));

        var confirmationToken = Guid.NewGuid();
        var result = await accountRepository.AddAccountAndConfirmationToken(account, 
            confirmationTokenHash: hasher.GenerateHash(confirmationToken.ToString()));
        
        { 
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Confirmation token: {confirmationToken}");
            Console.BackgroundColor = default;
        }
        // send command to notification

        return result.IsSuccess 
            ? Result.Ok(new CreateAccountResponse(account.Id)) 
            : Result.Fail(result.Errors);
    }
}