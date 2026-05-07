namespace PersonalKnowledge.Domain.Entities;

public class Conversation : Entity
{
    public string Title { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public ConversationSource ConversationSource { get; set; }
    public ICollection<Message> Messages { get; set; }
}