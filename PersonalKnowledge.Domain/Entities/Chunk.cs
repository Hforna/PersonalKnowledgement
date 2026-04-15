namespace PersonalKnowledge.Domain.Entities;

public class Chunk : Entity
{
    public Guid DocumentId { get; set; }   
    public string Text { get; set; }       
    public int ChunkIndex { get; set; }    
    public Document Document { get; set; } 
}