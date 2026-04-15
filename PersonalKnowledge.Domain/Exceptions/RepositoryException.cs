namespace PersonalKnowledge.Domain.Exceptions;

public class RepositoryException : ApplicationException
{
    public string EntityType { get; }

    public RepositoryException(string entityType, string message) 
        : base($"Repository error for entity '{entityType}': {message}") 
    {
        EntityType = entityType;
    }

    public RepositoryException(string entityType, string message, Exception innerException) 
        : base($"Repository error for entity '{entityType}': {message}", innerException) 
    {
        EntityType = entityType;
    }
}

