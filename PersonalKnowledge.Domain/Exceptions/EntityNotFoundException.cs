namespace PersonalKnowledge.Domain.Exceptions;

public class EntityNotFoundException : ApplicationException
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityType) 
        : base($"Entity of type '{entityType}' was not found.") 
    {
        EntityType = entityType;
    }

    public EntityNotFoundException(string entityType, object entityId) 
        : base($"Entity of type '{entityType}' with ID '{entityId}' was not found.") 
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, string message) 
        : base(message) 
    {
        EntityType = entityType;
    }
}

