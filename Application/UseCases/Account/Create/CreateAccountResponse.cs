namespace Application.UseCases.Account.Create;

public class CreateAccountResponse(Guid accountId)
{
    public Guid AccountId { get; } = accountId;
}