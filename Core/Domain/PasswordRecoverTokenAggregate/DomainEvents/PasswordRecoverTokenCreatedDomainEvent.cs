using Core.Domain.SharedKernel;

namespace Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

public record PasswordRecoverTokenCreatedDomainEvent : DomainEvent
{
    public PasswordRecoverTokenCreatedDomainEvent(Guid accountId, Guid recoverToken)
    {
        if (accountId == Guid.Empty) throw new ArgumentException($"{nameof(accountId)} cannot be empty");
        if (recoverToken == Guid.Empty) throw new ArgumentException($"{nameof(recoverToken)} cannot be empty");

        AccountId = accountId;
        RecoverToken = recoverToken;
    }

    public Guid RecoverToken { get; private set; }
    public Guid AccountId { get; private set; }
};