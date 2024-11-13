namespace Domain.Exceptions;

public class InvalidPhoneException(string description) 
    : DomainException(description);