using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

public record PasswordRecoverTokenCreatedDomainEvent : DomainEvent
{
    public Guid RecoverToken { get; init; }
    
    public string Email { get; init; }
    
    public PasswordRecoverTokenCreatedDomainEvent(Guid recoverToken, string email)
    {
        if (recoverToken == Guid.Empty) throw new ValueIsRequiredException($"{nameof(recoverToken)} cannot be empty");
        if (string.IsNullOrWhiteSpace(email)) throw new ValueIsRequiredException($"{nameof(email)} cannot be empty or null");
        
        RecoverToken = recoverToken;
        Email = email;
    }
};