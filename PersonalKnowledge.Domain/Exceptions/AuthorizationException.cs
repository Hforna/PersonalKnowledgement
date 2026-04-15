namespace PersonalKnowledge.Domain.Exceptions;

public class AuthorizationException : ApplicationException
{
    public string? Resource { get; }

    public AuthorizationException(string message = "You do not have permission to access this resource.") 
        : base(message) { }

    public AuthorizationException(string resource, string message) 
        : base($"Access denied to resource '{resource}'. {message}") 
    {
        Resource = resource;
    }

    public AuthorizationException(string message, Exception innerException) 
        : base(message, innerException) { }
}

