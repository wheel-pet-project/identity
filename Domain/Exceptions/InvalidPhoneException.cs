namespace Domain.Exceptions;

public class InvalidPhoneException(string description, Exception? innerException = null) 
    : DomainException(description, innerException);