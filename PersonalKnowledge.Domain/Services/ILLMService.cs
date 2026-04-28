namespace PersonalKnowledge.Domain.Services;

public interface ILLMService
{
    public Task<string> GenerateResponseByContext(string context, string prompt);
    public Task<string> DescribeImage(string imageUrl);
    public Task<string> DescribeVideo(string videoUrl);
    public Task<string> ProcessAudio(string audioUrl);
    public Task<bool> IsTextQuestion(string text);
}