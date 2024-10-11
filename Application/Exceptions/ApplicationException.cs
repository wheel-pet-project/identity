namespace Application.Exceptions;

public class ApplicationException(string title, string description) : Exception
{
    public string Title { get; private set; } = title;

    public string Description { get; private set; } = description;
}