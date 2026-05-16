namespace PersonalKnowledge.Domain.Services;

public interface ISenderResolver
{
    public Task<string> ResolveMessageSending(string prompt, Guid userId);
}