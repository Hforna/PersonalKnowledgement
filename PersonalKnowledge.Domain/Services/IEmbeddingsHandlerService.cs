namespace PersonalKnowledge.Domain.Services;

public interface IEmbeddingsHandlerService
{
    public Task<ReadOnlyMemory<float>> GenerateEmbedding(string chunks, string label = "");
}