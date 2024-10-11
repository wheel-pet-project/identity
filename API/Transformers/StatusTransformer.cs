using Proto.Identity;

namespace API.Transformers;

public class StatusTransformer
{
    public Status ToResponse(int statusId) =>
        statusId switch
        {
            0 => Status.ApprovedUnspecified,
            1 => Status.PendingConfirmation,
            2 => Status.PendingApproval,
            3 => Status.Deactivated,
            _ => throw new ArgumentOutOfRangeException(nameof(statusId), 
                statusId, "Invalid role")
        };

    public Status ToResponse(Domain.AccountAggregate.Status status)
    {
        var result = Status.ApprovedUnspecified;
        status.When(Domain.AccountAggregate.Status.Approved)
            .Then(() => result = Status.ApprovedUnspecified);
        status.When(Domain.AccountAggregate.Status.PendingConfirmation)
            .Then(() => result = Status.PendingConfirmation);
        status.When(Domain.AccountAggregate.Status.PendingApproval)
            .Then(() => result = Status.PendingApproval);
        status.When(Domain.AccountAggregate.Status.Deactivated)
            .Then(() => result = Status.Deactivated);

        return result;
    }
}