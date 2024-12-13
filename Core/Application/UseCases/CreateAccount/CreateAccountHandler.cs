using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.Services;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.CreateAccount;

public class CreateAccountHandler(
    IConfirmationTokenRepository confirmationTokenRepository,
    IAccountRepository accountRepository,
    ICreateAccountService createAccountService,
    IUnitOfWork unitOfWork,
    IHasher hasher) 
    : IRequestHandler<CreateAccountRequest, Result<CreateAccountResponse>>
{
    public async Task<Result<CreateAccountResponse>> Handle(CreateAccountRequest request, CancellationToken _)
    {
        var creatingAccountResult = await createAccountService.CreateUser(request.Role, request.Email, request.Phone,
            hasher.GenerateHash(request.Password));
        if (creatingAccountResult.IsFailed)
            return Result.Fail(creatingAccountResult.Errors);
        var account = creatingAccountResult.Value;
        
        var confirmationTokenGuid = Guid.NewGuid();
        var confirmationToken = ConfirmationToken.Create(account.Id,
            confirmationTokenHash: hasher.GenerateHash(confirmationTokenGuid.ToString()));
        
        await unitOfWork.BeginTransaction();
        await accountRepository.Add(account);
        await confirmationTokenRepository.Add(confirmationToken);
        var transactionResult = await unitOfWork.Commit();
        
        // send command to notification
        { 
            Console.WriteLine($"Confirmation token: {confirmationTokenGuid.ToString()}");
        }

        return transactionResult.IsSuccess
            ? new CreateAccountResponse(account.Id)
            : transactionResult;
    }
}