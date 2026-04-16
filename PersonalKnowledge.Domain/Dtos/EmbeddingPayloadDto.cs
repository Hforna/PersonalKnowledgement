namespace PersonalKnowledge.Domain.Dtos;

public class EmbeddingPayloadDto
{
    public string text { get; set; }
    public Guid document_id { get; set; }
    public Guid user_id { get; set; }
}