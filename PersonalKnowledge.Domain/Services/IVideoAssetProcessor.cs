namespace PersonalKnowledge.Domain.Services;

public interface IVideoAssetProcessor
{
    public Task ProcessAsset(Guid assetId);
}