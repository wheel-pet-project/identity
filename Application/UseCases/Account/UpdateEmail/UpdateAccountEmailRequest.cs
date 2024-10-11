namespace Application.UseCases.Account.UpdateEmail;

public class UpdateAccountEmailRequest(Guid correlationId, Guid accountId, string email)
{
    public Guid CorrelationId { get; init; } = correlationId;

    public Guid AccountId { get; init; } = accountId;

    public string Email { get; init; } = email;
}