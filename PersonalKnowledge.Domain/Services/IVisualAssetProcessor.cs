namespace PersonalKnowledge.Domain.Services;

public interface IVisualAssetProcessor
{
    public Task ProcessAsset(Guid assetId);
}