using Domain.AccountAggregate;

namespace Application.Application.UseCases.Account.Authorize;

public record AuthorizeAccountResponse(
    Guid AccountId, 
    Role Role, 
    Status Status);