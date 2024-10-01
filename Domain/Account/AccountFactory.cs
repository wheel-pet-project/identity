using Domain.Exceptions;

namespace Domain.Account;

public class AccountFactory
{
    public Account CreateAccount(
        Guid id,
        int roleId,
        string email,
        string phone,
        string password,
        bool isActive = true,
        bool isDeleted = false)
    {
        var errors = EntityValidate(id, roleId, email, phone, password);
        if (errors.Any() == false)
            return new Account(id, roleId, email, phone, password, isActive, isDeleted);
        
        throw new FactoryException(errors);
    }

    private IEnumerable<DomainException> EntityValidate(
        Guid id,
        int roleId,
        string email,
        string phone,
        string password)
    {
        var errors = new List<DomainException>();

        if (id == Guid.Empty)
            errors.Add(new DomainException("Id is required",
                "Id is invalid or empty"));

        if (roleId is < 0 or > 4)              // TODO: add roles validation
            errors.Add(new DomainException("RoleId is required",
                "RoleId is invalid or empty"));

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