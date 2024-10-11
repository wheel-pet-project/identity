using Domain.AccountAggregate;

namespace Application.UseCases.Account.Create;

public class CreateAccountRequest(
    Guid correlationId,
    Role role,
    string email,
    string phone,
    string password)
{
    public Guid CorrelationId { get; } = correlationId;

    public Role Role { get; } = role;

    public string Email { get; } = email;

    public string Phone { get; } = phone;

    public string Password { get; } = password;
}