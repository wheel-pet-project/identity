using Core.Domain.AccountAggregate;
using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.Services.CreateAccountService;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.CreateAccount;

public class CreateAccountHandler(
    IConfirmationTokenRepository confirmationTokenRepository,
    ICreateAccountService createAccountService,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    IHasher hasher)
    : IRequestHandler<CreateAccountCommand, Result<CreateAccountResponse>>
{
    public async Task<Result<CreateAccountResponse>> Handle(CreateAccountCommand command, CancellationToken _)
    {
        var confirmationTokenGuid = Guid.NewGuid();
        var creatingAccountResult = await createAccountService.CreateUser(command.Role, command.Email, command.Phone,
            command.Password, confirmationTokenGuid);
        if (creatingAccountResult.IsFailed) return Result.Fail(creatingAccountResult.Errors);

        var account = creatingAccountResult.Value;
        var confirmationToken =
            ConfirmationToken.Create(account.Id, hasher.GenerateHash(confirmationTokenGuid.ToString()));

        var transactionResult = await SaveInTransaction(async () =>
        {
            await accountRepository.Add(account);
            await confirmationTokenRepository.Add(confirmationToken);
            await outbox.PublishDomainEvents(account);
        });

        return transactionResult.IsSuccess
            ? MapToResponse(account)
            : transactionResult;
    }

    private async Task<Result> SaveInTransaction(Func<Task> execute)
    {
        await unitOfWork.BeginTransaction();

        await execute();

        return await unitOfWork.Commit();
    }

    private CreateAccountResponse MapToResponse(Account account)
    {
        return new CreateAccountResponse(account.Id);
    }
}