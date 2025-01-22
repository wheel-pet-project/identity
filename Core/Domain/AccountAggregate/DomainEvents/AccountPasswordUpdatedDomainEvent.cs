using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.AccountAggregate.DomainEvents;

public record AccountPasswordUpdatedDomainEvent : DomainEvent
{
    public Guid AccountId { get; init; }

    public AccountPasswordUpdatedDomainEvent(Guid accountId)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        
        AccountId = accountId;
    }
}