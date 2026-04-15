namespace PersonalKnowledge.Domain.Exceptions;

public class DocumentException : ApplicationException
{
    public DocumentException(string message) : base(message) { }
    public DocumentException(string message, Exception innerException) : base(message, innerException) { }
}

