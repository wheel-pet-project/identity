namespace Application.UseCases.CreateAccount;

public class CreateAccountResponse(Guid correlationId, Guid accountId)
{
    public Guid CorrelationId { get; } = correlationId;
    
    public Guid AccountId { get; } = accountId;
}