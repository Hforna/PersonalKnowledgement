namespace PersonalKnowledge.Domain.Exceptions;

public class FileProcessingException : DocumentException
{
    public string FileName { get; }

    public FileProcessingException(string fileName, string message) 
        : base($"Error processing file '{fileName}': {message}") 
    {
        FileName = fileName;
    }

    public FileProcessingException(string fileName, string message, Exception innerException) 
        : base($"Error processing file '{fileName}': {message}", innerException) 
    {
        FileName = fileName;
    }
}

