namespace Domain.Exceptions;

public class InvalidStatusException(string description, Exception? innerException = null) 
    : DomainException(description, innerException);