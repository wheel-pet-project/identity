using System.Text.RegularExpressions;
using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;

namespace Core.Domain.AccountAggregate;

public class Account : Aggregate
{
    private Account()
    {
    }

    private Account(Role role, string email, string phone, string passwordHash)
        : this()
    {
        Id = Guid.NewGuid();
        Role = role;
        Status = Status.PendingConfirmation;
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
    }


    public Guid Id { get; private set; }
    public Role Role { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Status Status { get; private set; } = null!;


    public void SetStatus(Status potentialStatus)
    {
        if (!Status.All().Contains(potentialStatus))
            throw new ValueOutOfRangeException($"{nameof(potentialStatus)} cannot unsupported be null");
        // TODO: вынести проверку сверху в метод CanBeChangedToThisStatus
        if (!Status.CanBeChangedToThisStatus(potentialStatus))
            throw new DomainRulesViolationException($"{nameof(potentialStatus)} cannot be set to this account");

        Status = potentialStatus;
    }

    public void SetRole(Role potentialRole)
    {
        if (!Role.All().Contains(potentialRole))
            throw new ValueOutOfRangeException($"{nameof(potentialRole)} cannot be unsupported or null");
        if (!Role.CanBeChangedToThisRole(potentialRole))
            throw new DomainRulesViolationException($"{nameof(potentialRole)} cannot be set to this account");

        Role = potentialRole;
    }

    public void SetEmail(string newEmail)
    {
        if (!ValidateEmail(newEmail))
            throw new ValueIsInvalidException($"{nameof(newEmail)} cannot be invalid or null");
        Email = newEmail;
    }

    public void SetPhone(string newPhone)
    {
        if (!ValidatePhone(newPhone))
            throw new ValueIsInvalidException($"{nameof(newPhone)} cannot be invalid phone or null");
        Phone = newPhone;
    }

    public void SetPasswordHash(string newPasswordHash)
    {
        if (!ValidatePasswordHash(newPasswordHash))
            throw new ValueOutOfRangeException($"{newPasswordHash} cannot be invalid, hash length must be 60");

        PasswordHash = newPasswordHash;

        AddDomainEvent(new AccountPasswordUpdatedDomainEvent(Id));
    }

    public static Account Create(Role role, string email, string phone, string passwordHash, Guid confirmationTokenGuid)
    {
        if (!Role.All().Contains(role)) throw new ValueOutOfRangeException($"{nameof(role)} cannot be unknown or null");
        if (!ValidateEmail(email))
            throw new ValueIsInvalidException(
                $"{nameof(email)} cannot be invalid or null, email must be this view: someemail@mail.com");
        if (!ValidatePhone(phone))
            throw new ValueIsInvalidException(
                $"{nameof(phone)} cannot be invalid or null, phone must be this view: +79008007060, 79008007060 or 89008007060");
        if (!ValidatePasswordHash(passwordHash))
            throw new ValueOutOfRangeException(
                $"{nameof(passwordHash)} cannot be invalid, hash length must be 60");

        var newAccount = new Account(role, email, phone, passwordHash);
        newAccount.AddDomainEvent(new AccountCreatedDomainEvent(newAccount.Id, newAccount.Email, newAccount.Phone,
            confirmationTokenGuid));

        return newAccount;
    }

    public static bool ValidateNotHashedPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return password is not { Length: < 6 or > 30 };
    }

    private static bool ValidateEmail(string email)
    {
        if (email is null) throw new ValueIsRequiredException($"{nameof(email)} cannot be null");
        return Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    }

    private static bool ValidatePhone(string phone)
    {
        if (phone is null) throw new ValueIsRequiredException($"{nameof(phone)} cannot be null");
        return Regex.IsMatch(phone, @"(^8|7|\+7)((\d{10})|(\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}))");
    }

    private static bool ValidatePasswordHash(string passwordHash)
    {
        const int hashLength = 60;
        if (passwordHash is null) throw new ValueIsRequiredException($"{nameof(passwordHash)} cannot be null");
        return passwordHash.Length == hashLength;
    }
}