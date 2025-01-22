namespace Core.Domain.SharedKernel;

public interface IAggregate
{
    IReadOnlyList<DomainEvent> DomainEvents { get; }
}