namespace PersonalKnowledge.Domain.Entities;

public class Message : Entity
{
    public Guid ConversationId { get; set; }
    public string Role { get; set; }     
    public string Content { get; set; }
    public List<MessageSource> Sources { get; set; }
    public Conversation Conversation { get; set; }
}