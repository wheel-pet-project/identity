using Core.Domain.SharedKernel.Errors;
using Core.Domain.SharedKernel.Exceptions.InternalExceptions;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.ConfirmEmail;

public class ConfirmAccountEmailHandler(
    IConfirmationTokenRepository confirmationTokenRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IHasher hasher)
    : IRequestHandler<ConfirmAccountEmailCommand, Result>
{
    public async Task<Result> Handle(ConfirmAccountEmailCommand command, CancellationToken _)
    {
        var verifyingHashResult = await GetConfirmationTokenAndVerifyInpuHash(command);
        if (verifyingHashResult.IsFailed) return verifyingHashResult;

        var account = await accountRepository.GetById(command.AccountId, _);
        if (account is null)
            throw new DataConsistencyViolationException(
                "Data consistency violation: account that couldn't be found has been confirmed");

        account.Confirm();

        return await SaveInTransaction(async () => { await confirmationTokenRepository.Delete(command.AccountId); });
    }

    private async Task<Result> GetConfirmationTokenAndVerifyInpuHash(ConfirmAccountEmailCommand command)
    {
        var confirmationToken = await confirmationTokenRepository.Get(command.AccountId);

        if (confirmationToken is null) return Result.Fail(new NotFound("Confirmation token not found"));

        if (!hasher.VerifyHash(command.ConfirmationToken.ToString(), confirmationToken.ConfirmationTokenHash))
            return Result.Fail("Invalid confirmation token");

        return Result.Ok();
    }

    private async Task<Result> SaveInTransaction(Func<Task> execute)
    {
        await unitOfWork.BeginTransaction();

        await execute();

        return await unitOfWork.Commit();
    }
}