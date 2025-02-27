using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

public record PasswordRecoverTokenCreatedDomainEvent : DomainEvent
{
    public PasswordRecoverTokenCreatedDomainEvent(Guid accountId, Guid recoverToken)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        if (recoverToken == Guid.Empty) throw new ValueIsRequiredException($"{nameof(recoverToken)} cannot be empty");

        AccountId = accountId;
        RecoverToken = recoverToken;
    }

    public Guid RecoverToken { get; private set; }
    public Guid AccountId { get; private set; }
};