namespace PersonalKnowledge.Domain.Exceptions;

public class StorageException : ApplicationException
{
    public string? FilePath { get; }

    public StorageException(string message) 
        : base(message) { }

    public StorageException(string filePath, string message) 
        : base($"Storage error with file '{filePath}': {message}") 
    {
        FilePath = filePath;
    }

    public StorageException(string message, Exception innerException) 
        : base(message, innerException) { }

    public StorageException(string filePath, string message, Exception innerException) 
        : base($"Storage error with file '{filePath}': {message}", innerException) 
    {
        FilePath = filePath;
    }
}

