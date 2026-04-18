using PersonalKnowledge.Domain.Entities;

namespace PersonalKnowledge.Domain.Services;

public interface IAssetRepository
{
    public Task<IEnumerable<Chunk>> GetAssetChunksAsync(Guid assetId);   
    public Task<IEnumerable<Asset>> GetUserAssetsAsync(Guid userId);  
}