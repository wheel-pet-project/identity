using Core.Domain.SharedKernel;

namespace Core.Ports.Postgres;

public interface IOutbox
{
    Task PublishDomainEvents(IAggregate aggregate);
}