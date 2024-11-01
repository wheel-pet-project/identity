namespace Domain.Exceptions;

public class DomainException(string description) : Exception
{
    public string Description { get; private set; } = description;
}