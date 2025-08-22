using Core.Domain.SharedKernel;

namespace Core.Domain.AccountAggregate.DomainEvents;

public record AccountPasswordUpdatedDomainEvent : DomainEvent
{
    public Guid AccountId { get; private set; }

    public AccountPasswordUpdatedDomainEvent(Guid accountId)
    {
        if (accountId == Guid.Empty) throw new ArgumentException($"{nameof(accountId)} cannot be empty");

        AccountId = accountId;
    }
}