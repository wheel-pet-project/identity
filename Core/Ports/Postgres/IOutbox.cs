using Core.Domain.SharedKernel;
using FluentResults;

namespace Core.Ports.Postgres;

public interface IOutbox
{
    Task PublishDomainEvents(IAggregate aggregate);
}