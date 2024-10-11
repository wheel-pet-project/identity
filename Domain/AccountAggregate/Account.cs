using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.AccountAggregate;

public class Account
{
    public Account () {}
    
    internal Account(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; private set; }

    public Role Role { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string Phone { get; private set; } = null!;

    public string Password { get; private set; } = null!;

    public Status Status { get; private set; } = null!;

    
    public void SetStatus(Status status) => Status = status;
    
    public void SetRole(Role role) => Role = role;

    public void SetEmail(string newEmail)
    {
        if (Regex.IsMatch(
                newEmail,
                @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            Email = newEmail;
        else throw new DomainException(
                "Invalid email",
                "Email doesn't match the email regex pattern");
    }
    
    public void SetPhone(string newPhone)
    {
        if (Regex.IsMatch(
                newPhone,
                @"(^8|7|\+7)((\d{10})|(\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}))")) 
            Phone = newPhone;
        else throw new DomainException(
                "Invalid phone number",
                "Phone number doesn't match the phone regex pattern");
        
    }

    public void SetPassword(string newPassword)
    {
        if(newPassword.Length >= 6 && Password != newPassword)
            Password = newPassword;
        else throw new DomainException(
                "Invalid password",
                "Password must be at least 6 characters long and different from the current password");
    }
}