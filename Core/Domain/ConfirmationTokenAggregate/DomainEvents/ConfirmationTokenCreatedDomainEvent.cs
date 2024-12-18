using Core.Domain.SharedKernel;

namespace Core.Domain.ConfirmationTokenAggregate.DomainEvents;

public record ConfirmationTokenCreatedDomainEvent(Guid ConfirmationToken) : DomainEvent;