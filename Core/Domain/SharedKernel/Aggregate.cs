
namespace Core.Domain.SharedKernel;

public abstract class Aggregate : IAggregate
{
    private readonly List<DomainEvent> _domainEvents = [];
    
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}