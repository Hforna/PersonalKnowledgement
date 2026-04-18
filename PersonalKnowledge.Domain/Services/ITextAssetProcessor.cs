namespace PersonalKnowledge.Domain.Services;

public interface ITextAssetProcessor
{
    public Task ProcessAsset(Guid assetId);
}