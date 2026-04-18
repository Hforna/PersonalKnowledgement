using Microsoft.EntityFrameworkCore;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Persistence;

public class AssetRepository(DataContext context) : BaseRepository(context), IAssetRepository
{
    public async Task<IEnumerable<Chunk>> GetAssetChunksAsync(Guid assetId)
    {
        return await _context.Chunks.Where(d => d.AssetId == assetId).ToListAsync();
    }

    public async Task<IEnumerable<Asset>> GetUserAssetsAsync(Guid userId)
    {
        return await _context.Assets.Where(d => d.UserId == userId).ToListAsync();
    }
}