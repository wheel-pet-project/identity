namespace Domain.Exceptions;

public class InvalidPasswordException(string description) 
    : DomainException(description);