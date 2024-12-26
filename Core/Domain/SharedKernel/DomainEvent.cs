using MediatR;

namespace Core.Domain.SharedKernel;

public abstract record DomainEvent : INotification
{
    public Guid EventId { get; private set; } = Guid.NewGuid();
};