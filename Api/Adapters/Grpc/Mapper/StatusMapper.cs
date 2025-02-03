using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Proto.IdentityV1;
using DomainStatus = Core.Domain.AccountAggregate.Status;

namespace Api.Adapters.Grpc.Mapper;

public class StatusMapper
{
    public Status StatusToResponse(Core.Domain.AccountAggregate.Status domainStatus) =>
        domainStatus switch
        {
            _ when domainStatus == DomainStatus.Approved => Status.ApprovedUnspecified,
            _ when domainStatus == DomainStatus.PendingConfirmation => Status.PendingConfirmation,
            _ when domainStatus == DomainStatus.PendingApproval => Status.PendingApproval,
            _ when domainStatus == DomainStatus.Deactivated => Status.Deactivated,
            _ => throw new ValueOutOfRangeException($"{nameof(domainStatus)} is unknown status")
        };
}