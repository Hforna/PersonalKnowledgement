using Google.Protobuf.Collections;
using PersonalKnowledge.Domain.Dtos;
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

    public async Task<List<EmbeddingPayloadDto>> SearchSimilar(ReadOnlyMemory<float> embedding, Guid? userId = null, int limit = 5)
    {
        Filter? filter = null;
        if (userId.HasValue)
        {
            filter = new Filter();
            filter.Must.Add(new Condition { Field = new FieldCondition { Key = "user_id", Match = new Match { Text = userId.Value.ToString() } } });
        }

        var results = await _qdrantClient.SearchAsync(CollectionName, embedding.ToArray(), filter: filter, limit: (ulong)limit);

        return results.Select(r => new EmbeddingPayloadDto
        {
            text = r.Payload["text"].StringValue,
            document_id = Guid.Parse(r.Payload["document_id"].StringValue),
            user_id = Guid.Parse(r.Payload["user_id"].StringValue)
        }).ToList();
    }
}