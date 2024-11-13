using Proto.IdentityV1;
using DomainStatus = Domain.AccountAggregate.Status;

namespace API.Transformers;

public class StatusTransformer
{
    public Status ToResponse(DomainStatus s) =>
        s switch
        {
            DomainStatus status when status == DomainStatus.Approved => Status.ApprovedUnspecified,
            DomainStatus status when status == DomainStatus.PendingConfirmation => Status.PendingConfirmation,
            DomainStatus status when status == DomainStatus.PendingApproval => Status.PendingApproval,
            DomainStatus status when status == DomainStatus.Deactivated => Status.Deactivated,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown status")
        };
}