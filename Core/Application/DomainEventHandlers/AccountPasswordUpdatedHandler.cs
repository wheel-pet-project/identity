using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.SharedKernel.Errors;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class AccountPasswordUpdatedHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<AccountPasswordUpdatedDomainEvent>
{
    public async Task Handle(AccountPasswordUpdatedDomainEvent @event, CancellationToken cancellationToken)
    {
        var refreshTokens = await refreshTokenRepository.GetNotRevokedTokensByAccountId(@event.AccountId);

        if (refreshTokens.Count > 0)
        {
            await unitOfWork.BeginTransaction();

            foreach (var token in refreshTokens)
            {
                token.Revoke();
                await refreshTokenRepository.UpdateRevokeStatus(token);
            }

            var commitResult = await unitOfWork.Commit();
            if (commitResult.IsFailed) throw ((TransactionFail)commitResult.Errors[0]).Exception;
        }
    }
}