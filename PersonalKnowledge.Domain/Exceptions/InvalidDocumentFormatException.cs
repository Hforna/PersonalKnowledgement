namespace PersonalKnowledge.Domain.Exceptions;

public class InvalidDocumentFormatException : DocumentException
{
    public InvalidDocumentFormatException(string fileName, string message = "Invalid document format") 
        : base($"Document '{fileName}' has an invalid format. {message}") { }

    public InvalidDocumentFormatException(string fileName, Exception innerException) 
        : base($"Document '{fileName}' has an invalid format.", innerException) { }
}

