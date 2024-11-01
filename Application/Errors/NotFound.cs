using FluentResults;

namespace Application.Errors;

public class NotFound(string message) : IError
{
    public string Message { get; } = message;
    
    public Dictionary<string, object> Metadata { get; }
    
    public List<IError> Reasons { get; }
}