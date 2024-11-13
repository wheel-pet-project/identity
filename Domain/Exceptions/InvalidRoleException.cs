namespace Domain.Exceptions;

public class InvalidRoleException(string description, Exception? innerException = null) 
    : DomainException(description, innerException);