namespace PersonalKnowledge.Domain.Exceptions;

public class OperationException : ApplicationException
{
    public string OperationName { get; }

    public OperationException(string operationName, string message) 
        : base($"Operation '{operationName}' failed: {message}") 
    {
        OperationName = operationName;
    }

    public OperationException(string operationName, string message, Exception innerException) 
        : base($"Operation '{operationName}' failed: {message}", innerException) 
    {
        OperationName = operationName;
    }
}

