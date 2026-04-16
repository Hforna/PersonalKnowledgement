using PersonalKnowledge.Domain.Entities;

namespace PersonalKnowledge.Domain.Services;

public interface IDocumentRepository
{
    public Task<IEnumerable<Chunk>> GetDocumentChunksAsync(Guid documentId);   
    public Task<IEnumerable<Document>> GetUserDocumentsAsync(Guid userId);  
}