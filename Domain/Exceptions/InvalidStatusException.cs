namespace Domain.Exceptions;

public class InvalidStatusException(string description) : DomainException(description);