namespace Application.UseCases.Account.Authorize;

public class AuthorizeAccountResponse(Guid accountId, int role, int status)
{
    public Guid AccountId { get; init; } = accountId;

    public int RoleId { get; init; } = role;
    
    public int StatusId { get; init; } = status;
}