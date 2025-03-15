using Core.Domain.AccountAggregate.DomainEvents;
using Core.Ports.Postgres.Repositories;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class AccountPasswordUpdatedHandler(
    IRefreshTokenRepository refreshTokenRepository)
    : INotificationHandler<AccountPasswordUpdatedDomainEvent>
{
    public async Task Handle(AccountPasswordUpdatedDomainEvent @event, CancellationToken cancellationToken)
    {
        var accountId = @event.AccountId;

        var refreshTokens = await refreshTokenRepository.GetNotRevokedTokensByAccountId(accountId);

        foreach (var token in refreshTokens)
        {
            token.Revoke();
            await refreshTokenRepository.UpdateRevokeStatus(token);
        }
    }
}