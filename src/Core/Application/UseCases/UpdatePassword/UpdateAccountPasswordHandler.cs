using Core.Domain.Services.UpdateAccountPasswordService;
using Core.Domain.SharedKernel.Errors;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.UpdatePassword;

public class UpdateAccountPasswordHandler(
    IUpdateAccountPasswordService updateAccountPasswordService,
    IPasswordRecoverTokenRepository passwordRecoverTokenRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    IHasher hasher,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateAccountPasswordCommand, Result>
{
    public async Task<Result> Handle(UpdateAccountPasswordCommand command, CancellationToken _)
    {
        var account = await accountRepository.GetByEmail(command.Email, _);
        if (account == null) return Result.Fail(new NotFound("Account not found"));

        var passwordRecoverToken = await passwordRecoverTokenRepository.Get(account.Id);
        if (passwordRecoverToken == null) return Result.Fail(new NotFound("Password recover token not found"));

        if (!passwordRecoverToken.IsValid(timeProvider))
            return Result.Fail("Password recover token has expired or already applied");
        if (!hasher.VerifyHash(command.RecoverToken.ToString(), passwordRecoverToken.RecoverTokenHash))
            return Result.Fail("Invalid reset password token");

        passwordRecoverToken.Apply();
        updateAccountPasswordService.UpdatePassword(account, command.NewPassword);

        return await SaveInTransaction(async () =>
        {
            await accountRepository.UpdatePasswordHash(account);
            await passwordRecoverTokenRepository.UpdateAppliedStatus(passwordRecoverToken);
            await outbox.PublishDomainEvents(account);
        });
    }

    private async Task<Result> SaveInTransaction(Func<Task> execute)
    {
        await unitOfWork.BeginTransaction();

        await execute();

        return await unitOfWork.Commit();
    }
}