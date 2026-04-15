namespace PersonalKnowledge.Domain.Entities;

public class Conversation : Entity
{
    public string Title { get; set; }
    public ICollection<Message> Messages { get; set; }
}