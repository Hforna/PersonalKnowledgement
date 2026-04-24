namespace PersonalKnowledge.Domain.Services;

public interface IAudioAssetProcessor
{
    public Task ProcessAsset(Guid assetId);
}