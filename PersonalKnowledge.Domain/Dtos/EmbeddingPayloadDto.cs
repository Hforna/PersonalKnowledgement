namespace PersonalKnowledge.Domain.Dtos;

public class EmbeddingPayloadDto
{
    public string text { get; set; }
    public string label { get; set; }
    public Guid asset_id { get; set; }
    public Guid user_id { get; set; }
}