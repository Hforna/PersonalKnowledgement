using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Services;

public class SenderResolver : ISenderResolver
{
    private readonly ILLMService _llmService;

    public SenderResolver(ILLMService llmService)
    {
        _llmService = llmService;
    }


    public async Task<string> ResolveMessageSending(string prompt, Guid userId)
    {
        return await _llmService.ProcessText(prompt, userId);
    }
}