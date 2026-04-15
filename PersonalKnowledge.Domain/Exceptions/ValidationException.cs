namespace PersonalKnowledge.Domain.Exceptions;

public class ValidationException : ApplicationException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) 
        : base(message) 
    {
        Errors = new();
    }

    public ValidationException(Dictionary<string, string[]> errors) 
        : base("One or more validation failures have occurred.") 
    {
        Errors = errors;
    }

    public ValidationException(string fieldName, string message) 
        : base(message) 
    {
        Errors = new() { { fieldName, new[] { message } } };
    }
}

