namespace Domain.Exceptions;

public class FactoryException(List<DomainException> errors) : Exception
{
    public List<DomainException> Errors { get; private set; } 
        = errors;
}