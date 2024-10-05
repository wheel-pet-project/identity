using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.AccountAggregate;

public class Account
{
    internal Account(Guid id,
        Role role,
        string email,
        string phone,
        string password,
        Status status)
    {
        Id = id;
        Role = role;
        Email = email;
        Phone = phone;
        Password = password;
        Status = status;
    }

    public Guid Id { get; private set; }

    public Role Role { get; private set; }

    public string Email { get; private set; }

    public string Phone { get; private set; }

    public string Password { get; private set; }

    public Status Status { get; private set; }
    
    
    public void ChangeEmail(string newEmail)
    {
        if (Regex.IsMatch(
                newEmail,
                @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            Email = newEmail;
        else throw new DomainException(
                "Invalid email",
                "Email doesn't match the email regex pattern");
    }
    
    public void ChangePhone(string newPhone)
    {
        if (Regex.IsMatch(
                newPhone,
                @"(^8|7|\+7)((\d{10})|(\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}))")) 
            Phone = newPhone;
        else throw new DomainException(
                "Invalid phone number",
                "Phone number doesn't match the phone regex pattern");
        
    }

    public void ChangePassword(string newPassword)
    {
        if(newPassword.Length >= 6 && Password != newPassword)
            Password = newPassword;
        else throw new DomainException(
                "Invalid password",
                "Password must be at least 6 characters long and different from the current password");
    }

    public void Activate() => Confirm();
    
    public void DisActivate() => Status = Status.Deactivated;
    
    public void Confirm() => Status = Status.PendingApproval;
    
    public void UnConfirm() => Status = Status.PendingVerification;
    
    public void MarkAsDeleted() => Status = Status.Deleted;
}