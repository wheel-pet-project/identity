namespace Domain.Exceptions;

public class DomainException(string description, Exception? innerException = null) : Exception
{
    public string Description { get; } = description;

    public Exception? InnerException { get; } = innerException;
}