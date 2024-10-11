using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.AccountAggregate;

public class AccountFactory
{
    public Account CreateAccount(
        Role role,
        Status status,
        string email,
        string phone,
        string password)
    {
        var account = new Account(Guid.NewGuid());
        account.SetRole(role);
        account.SetStatus(status);
        account.SetEmail(email);
        account.SetPhone(phone);
        account.SetPassword(password);
        
        return account;
    }
}