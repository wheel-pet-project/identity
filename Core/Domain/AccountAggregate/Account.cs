using System.Text.RegularExpressions;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;

namespace Core.Domain.AccountAggregate;

public class Account
{
    private Account(){}
    
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
        if (!Status.All().Contains(potentialStatus)) throw new ValueOutOfRangeException("Unknown status or null");
        if (!Status.CanBeChangedToThisStatus(potentialStatus)) 
            throw new DomainRulesViolationException("This status cannot be set to this account");
        
        Status = potentialStatus;
    }

    public void SetRole(Role potentialRole)
    {
        if (!Role.All().Contains(potentialRole)) throw new ValueOutOfRangeException("Unknown role or null");
        if (!Role.CanBeChangedToThisRole(potentialRole)) 
            throw new DomainRulesViolationException("This role cannot be set to this account");
        
        Role = potentialRole;
    }

    public void SetEmail(string newEmail)
    {
        if (!ValidateEmail(newEmail)) throw new ValueIsInvalidException("Invalid email or null");
        Email = newEmail;
    }
    
    public void SetPhone(string newPhone)
    {
        if (!ValidatePhone(newPhone)) throw new ValueIsInvalidException("Invalid phone or null");
        Phone = newPhone;
    }

    public void SetPasswordHash(string newPasswordHash)
    {
        if (!ValidatePassword(newPasswordHash)) 
            throw new ValueOutOfRangeException("Password hash is invalid, hash length must be 60");
        PasswordHash = newPasswordHash;
    }
    
    public static Account Create(Role role, string email, string phone,
        string passwordHash)
    {
        if (!Role.All().Contains(role)) throw new ValueOutOfRangeException("Unknown role or null");
        if (!ValidateEmail(email)) throw new ValueIsInvalidException(
            "Email is invalid or null, email must be this view: someemail@mail.com");
        if (!ValidatePhone(phone)) throw new ValueIsInvalidException(
            "Phone is invalid or null, phone must be this view: +79008007060, 79008007060 or 89008007060");
        if (!ValidatePassword(passwordHash)) throw new ValueOutOfRangeException(
            "Password hash is invalid, hash length must be 60");
        
        return new Account(role, email, phone, passwordHash);
    }
    
    private static bool ValidateEmail(string email)
    {
        if (email is null) throw new ValueIsRequiredException("Email is null");
        return Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    }

    private static bool ValidatePhone(string phone)
    {
        if (phone is null) throw new ValueIsRequiredException("Phone is null");
        return Regex.IsMatch(phone, @"(^8|7|\+7)((\d{10})|(\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}))");
    }

    private static bool ValidatePassword(string passwordHash)
    {
        const int hashLength = 60;
        if (passwordHash is null) throw new ValueIsRequiredException("Password hash is null");
        return passwordHash.Length == hashLength;
    }
}
