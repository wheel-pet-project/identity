using Proto.IdentityV1;
using DomainStatus = Core.Domain.AccountAggregate.Status;

namespace Api.Mapper;

public class StatusMapper
{
    public Status StatusToResponse(Core.Domain.AccountAggregate.Status s) =>
        s switch
        {
            var status when status == DomainStatus.Approved => Status.ApprovedUnspecified,
            var status when status == DomainStatus.PendingConfirmation => Status.PendingConfirmation,
            var status when status == DomainStatus.PendingApproval => Status.PendingApproval,
            var status when status == DomainStatus.Deactivated => Status.Deactivated,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown status")
        };
}