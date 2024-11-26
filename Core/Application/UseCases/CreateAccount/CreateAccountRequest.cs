using Core.Domain.AccountAggregate;

namespace Core.Application.UseCases.Create;

public record CreateAccountRequest(
    Guid CorrelationId,
    Role Role,
    string Email,
    string Phone,
    string Password);