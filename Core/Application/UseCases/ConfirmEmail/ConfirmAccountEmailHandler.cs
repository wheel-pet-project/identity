using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
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
        var confirmationToken = await confirmationTokenRepository.Get(command.AccountId);
        if (confirmationToken is null) return Result.Fail(new NotFound("Confirmation token not found"));

        if (!hasher.VerifyHash(command.ConfirmationToken.ToString(), confirmationToken.ConfirmationTokenHash))
            return Result.Fail("Invalid confirmation token");

        var account = await accountRepository.GetById(command.AccountId);
        if (account is null)
            throw new DataConsistencyViolationException(
                "Data consistency violation: account that couldn't be found has been confirmed");

        account.SetStatus(Status.PendingApproval);

        await unitOfWork.BeginTransaction();
        await confirmationTokenRepository.Delete(command.AccountId);
        await accountRepository.UpdateStatus(account);

        return await unitOfWork.Commit();
    }
}