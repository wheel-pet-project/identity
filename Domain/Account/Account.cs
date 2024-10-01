using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.Account;

public class Account
{
    internal Account(Guid id,
        int roleId,
        string email,
        string phone,
        string password,
        bool isActive,
        bool isDeleted)
    {
        Id = id;
        RoleId = roleId;
        Email = email;
        Phone = phone;
        Password = password;
        IsActive = isActive;
        IsDeleted = isDeleted;
    }

    public Guid Id { get; private set; }

    public int RoleId { get; private set; }

    public string Email { get; private set; }

    public string Phone { get; private set; }

    public string Password { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }
    
    
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

    public void Activate() => IsActive = true;
    
    public void DisActivate() => IsActive = false;
    
    public void MarkAsDeleted() => IsDeleted = true;
}