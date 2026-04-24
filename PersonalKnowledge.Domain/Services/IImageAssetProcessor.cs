namespace PersonalKnowledge.Domain.Services;

public interface IImageAssetProcessor
{
    public Task ProcessAsset(Guid assetId);
}