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
        var account = await accountRepository.GetByEmail(command.Email);
        if (account == null) return Result.Fail(new NotFound($"account with this {nameof(command.Email)} not found"));

        var recoverToken = Guid.NewGuid();
        var passwordRecoverToken = PasswordRecoverToken.Create(account, recoverToken,
            hasher.GenerateHash(recoverToken.ToString()), timeProvider);

        await unitOfWork.BeginTransaction();
        await passwordRecoverTokenRepository.Add(passwordRecoverToken);
        await outbox.PublishDomainEvents(passwordRecoverToken);

        return await unitOfWork.Commit();
    }
}