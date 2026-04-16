namespace PersonalKnowledge.Domain.Services;

public interface ILLMService
{
    public Task<string> GenerateResponseByContext(string context, string prompt);
}