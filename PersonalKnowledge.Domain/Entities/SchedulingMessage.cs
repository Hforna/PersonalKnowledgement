namespace PersonalKnowledge.Domain.Entities;

public class SchedulingMessage : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string Content { get; set; }
    public DateTime ScheduledFor { get; set; }
    public bool IsProcessed { get; set; }
    public string? RecurringExpression { get; set; } = null;
}