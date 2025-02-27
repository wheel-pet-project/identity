using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.AccountAggregate.DomainEvents;

public record AccountCreatedDomainEvent : DomainEvent
{
    public AccountCreatedDomainEvent(Guid accountId, string email, string phone, Guid confirmationToken)
    {
        if (accountId == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(accountId)} cannot be empty");
        if (confirmationToken == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(confirmationToken)} cannot be empty");
        if (string.IsNullOrWhiteSpace(email))
            throw new ValueIsRequiredException($"{nameof(email)} cannot be empty or null");
        if (string.IsNullOrWhiteSpace(phone))
            throw new ValueIsRequiredException($"{nameof(phone)} cannot be empty or null");

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