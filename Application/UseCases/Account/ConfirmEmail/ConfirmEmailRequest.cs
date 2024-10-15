namespace Application.UseCases.Account.ConfirmEmail;

public class ConfirmEmailRequest(Guid accountId, Guid confirmationId)
{
    public Guid AccountId { get; private set; } = accountId;
    
    public Guid ConfirmationId { get; private set; } = confirmationId;
}