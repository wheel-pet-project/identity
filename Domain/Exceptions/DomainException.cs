using Microsoft.AspNetCore.Http;

namespace Domain.Exceptions;

public class DomainException(
    string title, 
    string description, 
    int code = StatusCodes.Status500InternalServerError) : Exception
{
    public string Title { get; private set; } = title;

    public string Description { get; private set; } = description;

    public int Code { get; private set; } = code;
}