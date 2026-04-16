using PersonalKnowledge.Domain.Dtos;

namespace PersonalKnowledge.Domain.Services;

public interface IVectorDatabaseService
{
    public Task InsertEmbedding(Guid chunkId, ReadOnlyMemory<float> embedding, Dictionary<string, string> payload);
    public Task<List<EmbeddingPayloadDto>> SearchSimilar(ReadOnlyMemory<float> embedding, Guid? userId = null, int limit = 5);
}