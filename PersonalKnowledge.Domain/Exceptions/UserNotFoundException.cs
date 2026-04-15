namespace PersonalKnowledge.Domain.Exceptions;

public class UserNotFoundException : UserException
{
    public UserNotFoundException(string identifier) 
        : base($"User with identifier '{identifier}' was not found.") { }

    public UserNotFoundException(Guid userId) 
        : base($"User with ID '{userId}' was not found.") { }
}

