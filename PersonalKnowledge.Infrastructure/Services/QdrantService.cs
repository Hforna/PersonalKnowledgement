using PersonalKnowledge.Domain.Services;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace PersonalKnowledge.Infrastructure.Services;

public class QdrantService : IVectorDatabaseService
{
    private readonly IQdrantClient _qdrantClient;
    private const string CollectionName = "documents";

    public QdrantService(IQdrantClient qdrantClient)
    {
        _qdrantClient = qdrantClient;
    }

    public async Task InsertEmbedding(Guid chunkId, ReadOnlyMemory<float> embedding, Dictionary<string, string> payload)
    {
        if (!await _qdrantClient.CollectionExistsAsync(CollectionName))
            await _qdrantClient.CreateCollectionAsync(CollectionName,
                new VectorParams { Size = 1536, Distance = Distance.Cosine });
        
        var point = new PointStruct
        {
            Id = chunkId,
            Vectors = embedding.ToArray()
        };

        foreach (var item in payload)
        {
            point.Payload.Add(item.Key, item.Value);
        }
        
        await _qdrantClient.UpsertAsync(CollectionName, new[] { point });
    }
}