namespace PersonalKnowledge.Domain.Services;

public interface IDocumentRepository
{
    public Task<IEnumerable<Chunk>> GetDocumentChunksAsync(Guid documentId);   
}