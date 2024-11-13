using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.AccountAggregate;

public class Account
{
    protected internal Account () {}
    
    public Account(
        Role role,
        string email,
        string phone,
        string password) : this()
    {
        Id = Guid.NewGuid();
        SetRole(role);
        SetStatus(Status.PendingConfirmation);
        SetEmail(email);
        SetPhone(phone);
        SetPassword(password);
    }

    public Guid Id { get; private set; }

    public Role Role { get; private set; }

    public string Email { get; private set; } = null!;

    public string Phone { get; private set; } = null!;

    public string Password { get; private set; } = null!;

    public Status Status { get; private set; }

    
    public void SetStatus(Status newStatus)
    {
        if (Status is null || Status.CanSetThisStatus(newStatus)) 
            Status = newStatus;
        else
            throw new InvalidStatusException("Cannot set this status to this account");
    }

    public void SetRole(Role role)
    {
        if (Role is null || Role.CanSetThisRole(role))
            Role = role;
        else
            throw new InvalidRoleException("Cannot set this role to this account");
    }

    public void SetEmail(string newEmail)
    {
        if (Regex.IsMatch(
                newEmail,
                @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            Email = newEmail;
        else throw new InvalidEmailException("Not matching with regex pattern");
    }
    
    public void SetPhone(string newPhone)
    {
        if (Regex.IsMatch(
                newPhone,
                @"(^8|7|\+7)((\d{10})|(\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}))")) 
            Phone = newPhone;
        else throw new InvalidPhoneException("Not matching with regex pattern");
        
    }

    public void SetPassword(string newPassword)
    {
        const int hashLength = 60;
        if(newPassword.Length >= hashLength)
            Password = newPassword;
        else throw new InvalidPasswordException("Password field in aggregate must be hash from password");
    }
}