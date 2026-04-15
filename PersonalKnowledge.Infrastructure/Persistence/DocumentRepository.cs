using Microsoft.EntityFrameworkCore;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Persistence;

public class DocumentRepository(DataContext context) : BaseRepository(context), IDocumentRepository
{
    public async Task<IEnumerable<Chunk>> GetDocumentChunksAsync(Guid documentId)
    {
        return await _context.Chunks.Where(d => d.DocumentId == documentId).ToListAsync();
    }
}