using Domain.AccountAggregate;

namespace Application.Application.UseCases.Account.Create;

public record CreateAccountRequest(
    Guid CorrelationId,
    Role Role,
    string Email,
    string Phone,
    string Password);