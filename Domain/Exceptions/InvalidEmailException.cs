namespace Domain.Exceptions;

public class InvalidEmailException(string description) 
    : DomainException(description);