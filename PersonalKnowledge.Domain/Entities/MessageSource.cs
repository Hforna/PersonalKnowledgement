namespace PersonalKnowledge.Domain.Entities;

public class MessageSource : Entity
{
    public Guid MessageId { get; set; }
    public Guid ChunkId { get; set; }
    public float RelevanceScore { get; set; }
    public Message Message { get; set; }
    public Chunk Chunk { get; set; }
}
