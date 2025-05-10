using Core.Domain.SharedKernel;

namespace Core.Domain.AccountAggregate.DomainEvents;

public record AccountCreatedDomainEvent : DomainEvent
{
    public AccountCreatedDomainEvent(Guid accountId, string email, string phone, Guid confirmationToken)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException($"{nameof(accountId)} cannot be empty");
        if (confirmationToken == Guid.Empty)
            throw new ArgumentException($"{nameof(confirmationToken)} cannot be empty");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException($"{nameof(email)} cannot be empty or null");
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException($"{nameof(phone)} cannot be empty or null");

        AccountId = accountId;
        Email = email;
        Phone = phone;
        ConfirmationToken = confirmationToken;
    }

    public Guid AccountId { get; private set; }
    public Guid ConfirmationToken { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
}