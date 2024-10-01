namespace Domain.Exceptions;

public class FactoryException(IEnumerable<DomainException> errors) : Exception
{
    public IEnumerable<DomainException> Errors { get; private set; } 
        = errors;
}