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
    IHasher hasher)
    : IRequestHandler<RecoverAccountPasswordRequest, Result>
{
    public async Task<Result> Handle(RecoverAccountPasswordRequest request, CancellationToken _)
    {
        var account = await accountRepository.GetByEmail(request.Email);
        if (account == null) return Result.Fail(new NotFound("Account not found"));

        var recoverToken = Guid.NewGuid();
        var passwordRecoverToken = PasswordRecoverToken.Create(account, hasher.GenerateHash(recoverToken.ToString()));
        passwordRecoverToken.AddCreatedDomainEvent(recoverToken, account.Email);
        
        await unitOfWork.BeginTransaction();
        await passwordRecoverTokenRepository.Add(passwordRecoverToken);
        await outbox.PublishDomainEvents(passwordRecoverToken);
        
        return await unitOfWork.Commit();
    }
}