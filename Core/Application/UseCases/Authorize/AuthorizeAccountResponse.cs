using Core.Domain.AccountAggregate;

namespace Core.Application.UseCases.Authorize;

public record AuthorizeAccountResponse(
    Guid AccountId,
    Role Role,
    Status Status);