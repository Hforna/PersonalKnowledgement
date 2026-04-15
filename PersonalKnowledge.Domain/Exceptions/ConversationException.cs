namespace PersonalKnowledge.Domain.Exceptions;

public class ConversationException : ApplicationException
{
    public ConversationException(string message) 
        : base(message) { }

    public ConversationException(string message, Exception innerException) 
        : base(message, innerException) { }
}

