namespace PersonalKnowledge.Application.Responses;

public class ChatResponse
{
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = string.Empty;   
}