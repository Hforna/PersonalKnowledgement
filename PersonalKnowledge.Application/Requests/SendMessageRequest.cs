namespace PersonalKnowledge.Application.Requests;

public class SendMessageRequest
{
    public string Message { get; set; }
    public List<Guid>? AssetsIds { get; set; }
}