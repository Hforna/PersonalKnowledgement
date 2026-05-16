
using OpenAI.Chat;

namespace PersonalKnowledge.Domain.Services;

public interface ILLMService
{
    public Task<string> DescribeImage(string imageUrl);
    public Task<string> DescribeVideo(string videoUrl);
    public Task<string> ProcessAudio(string audioUrl);
    public Task<bool> IsTextQuestion(string text);
    public Task<string> ProcessText(string prompt, Guid userId);
}