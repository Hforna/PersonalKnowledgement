namespace PersonalKnowledge.Domain.Exceptions;

public class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message = "Authentication failed.") 
        : base(message) { }

    public AuthenticationException(string message, Exception innerException) 
        : base(message, innerException) { }
}

