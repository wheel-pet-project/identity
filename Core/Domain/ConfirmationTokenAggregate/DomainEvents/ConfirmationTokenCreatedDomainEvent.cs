using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.ConfirmationTokenAggregate.DomainEvents;

public record ConfirmationTokenCreatedDomainEvent : DomainEvent
{
    public Guid ConfirmationToken { get; private set; }
    
    public string Email { get; private set; }
    
    public ConfirmationTokenCreatedDomainEvent(Guid confirmationToken, string email)
    {
        if (confirmationToken == Guid.Empty) throw new ValueIsRequiredException($"{nameof(confirmationToken)} cannot be empty");
        if (string.IsNullOrWhiteSpace(email)) throw new ValueIsRequiredException($"{nameof(email)} cannot be empty or null");
        
        ConfirmationToken = confirmationToken;
        Email = email;
    }
};