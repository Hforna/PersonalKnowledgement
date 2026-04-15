namespace PersonalKnowledge.Domain.Exceptions;

public class UserException : ApplicationException
{
    public UserException(string message) 
        : base(message) { }

    public UserException(string message, Exception innerException) 
        : base(message, innerException) { }
}

