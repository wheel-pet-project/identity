namespace Application.Application.UseCases.Account.Authorize;

public record AuthorizeAccountResponse(
    Guid AccountId, 
    int Role, 
    int Status);