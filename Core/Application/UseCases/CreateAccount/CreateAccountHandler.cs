using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.Services;
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
    : IRequestHandler<CreateAccountRequest, Result<CreateAccountResponse>>
{
    public async Task<Result<CreateAccountResponse>> Handle(CreateAccountRequest request, CancellationToken _)
    {
        var confirmationTokenGuid = Guid.NewGuid();

        var creatingAccountResult = await createAccountService.CreateUser(request.Role, request.Email, request.Phone,
            request.Password, confirmationTokenGuid);
        if (creatingAccountResult.IsFailed) return Result.Fail(creatingAccountResult.Errors);
        var account = creatingAccountResult.Value;

        var confirmationToken =
            ConfirmationToken.Create(account.Id, hasher.GenerateHash(confirmationTokenGuid.ToString()));

        await unitOfWork.BeginTransaction();
        await accountRepository.Add(account);
        await confirmationTokenRepository.Add(confirmationToken);
        await outbox.PublishDomainEvents(account);

        var transactionResult = await unitOfWork.Commit();

        return transactionResult.IsSuccess
            ? new CreateAccountResponse(account.Id)
            : transactionResult;
    }
}