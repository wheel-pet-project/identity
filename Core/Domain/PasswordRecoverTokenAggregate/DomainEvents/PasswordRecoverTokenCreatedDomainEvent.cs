using Core.Domain.SharedKernel;

namespace Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

public record PasswordRecoverTokenCreatedDomainEvent(Guid RecoverToken) : DomainEvent;