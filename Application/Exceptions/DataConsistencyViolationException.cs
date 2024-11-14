namespace Application.Exceptions;

public class DataConsistencyViolationException(string title, string description) : ApplicationException(title, description);