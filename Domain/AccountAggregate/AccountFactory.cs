using Domain.Exceptions;

namespace Domain.AccountAggregate;

public class AccountFactory
{
    public Account CreateAccount(
        Guid id,
        Role role,
        string email,
        string phone,
        string password,
        Status status)
    {
        var errors = EntityValidate(id, email, phone, password);
        if (errors.Any() == false)
            return new Account(id, role, email, phone, password, status);
        
        throw new FactoryException(errors);
    }

    private List<DomainException> EntityValidate(
        Guid id,
        string email,
        string phone,
        string password)
    {
        var errors = new List<DomainException>();

        if (id == Guid.Empty)
            errors.Add(new DomainException("Id is required",
                "Id is invalid or empty"));

        if (string.IsNullOrEmpty(email))
            errors.Add(new DomainException("Email is required",
                "Email is invalid or empty"));

        if (string.IsNullOrEmpty(phone))
            errors.Add(new DomainException("Phone is required",
                "Phone is invalid or empty"));

        if (string.IsNullOrEmpty(password))
            errors.Add(new DomainException("Password is required",
                "Password is invalid or empty"));
        
        return errors;
    } 
}