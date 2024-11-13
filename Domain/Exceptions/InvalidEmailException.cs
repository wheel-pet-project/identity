namespace Domain.Exceptions;

public class InvalidEmailException(string description, Exception? innerException = null) 
    : DomainException(description, innerException);