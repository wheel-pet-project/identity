namespace Domain.Exceptions;

public class InvalidPasswordException(string description, Exception? innerException = null) 
    : DomainException(description, innerException);