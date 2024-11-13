namespace Domain.Exceptions;

public class InvalidRoleException(string description) : DomainException(description);