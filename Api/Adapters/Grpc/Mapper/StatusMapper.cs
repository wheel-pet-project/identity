using Core.Domain.SharedKernel.Exceptions.PublicExceptions;
using Proto.IdentityV1;
using DomainStatus = Core.Domain.AccountAggregate.Status;

namespace Api.Adapters.Grpc.Mapper;

public class StatusMapper
{
    public Status StatusToResponse(DomainStatus domainStatus)
    {
        return domainStatus switch
        {
            _ when domainStatus == DomainStatus.Confirmed => Status.ConfirmedUnspecified,
            _ when domainStatus == DomainStatus.PendingConfirmation => Status.PendingConfirmation,
            _ when domainStatus == DomainStatus.Deactivated => Status.Deactivated,
            _ => throw new ValueOutOfRangeException($"{nameof(domainStatus)} is unknown status")
        };
    }
}