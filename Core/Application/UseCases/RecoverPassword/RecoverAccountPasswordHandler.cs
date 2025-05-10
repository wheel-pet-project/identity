using Core.Domain.PasswordRecoverTokenAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RecoverPassword;

public class RecoverAccountPasswordHandler(
    IPasswordRecoverTokenRepository passwordRecoverTokenRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    IHasher hasher,
    TimeProvider timeProvider)
    : IRequestHandler<RecoverAccountPasswordCommand, Result>
{
    public async Task<Result> Handle(RecoverAccountPasswordCommand command, CancellationToken _)
    {
        var account = await accountRepository.GetByEmail(command.Email, _);
        if (account == null) return Result.Fail(new NotFound($"account with this {nameof(command.Email)} not found"));

        var recoverTokenGuid = Guid.NewGuid();
        var passwordRecoverToken = PasswordRecoverToken.Create(account, recoverTokenGuid,
            hasher.GenerateHash(recoverTokenGuid.ToString()), timeProvider);

        return await SaveInTransaction(async () =>
        {
            await passwordRecoverTokenRepository.Add(passwordRecoverToken);
            await outbox.PublishDomainEvents(passwordRecoverToken);
        });
    }

    private async Task<Result> SaveInTransaction(Func<Task> execute)
    {
        await unitOfWork.BeginTransaction();

        await execute();

        return await unitOfWork.Commit();
    }
}