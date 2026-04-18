namespace PersonalKnowledge.Domain.Entities;

public class Chunk : Entity
{
    public Guid AssetId { get; set; }   
    public string Text { get; set; }       
    public int ChunkIndex { get; set; }    
    public Asset Asset { get; set; } 
}