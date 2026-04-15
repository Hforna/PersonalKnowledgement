namespace PersonalKnowledge.Domain.Services;

public interface IVectorDatabaseService
{
    public Task InsertEmbedding(Guid chunkId, ReadOnlyMemory<float> embedding, Dictionary<string, string> payload);
}