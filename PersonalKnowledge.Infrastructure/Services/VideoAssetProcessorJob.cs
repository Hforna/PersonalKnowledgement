using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Services;

public class VideoAssetProcessorJob : IVideoAssetProcessor
{
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    private readonly IEmbeddingsHandlerService _embeddingsHandlerService;
    private readonly IVectorDatabaseService _vectorDatabaseService;
    private readonly ILogger<VideoAssetProcessorJob> _logger;
    private readonly ILLMService _llmService;

    public VideoAssetProcessorJob(IUnitOfWork uow, IStorageService storageService, 
        IEmbeddingsHandlerService embeddingsHandlerService, IVectorDatabaseService vectorDatabaseService, 
        ILogger<VideoAssetProcessorJob> logger, ILLMService llmService)
    {
        _uow = uow;
        _storageService = storageService;
        _embeddingsHandlerService = embeddingsHandlerService;
        _vectorDatabaseService = vectorDatabaseService;
        _logger = logger;       
        _llmService = llmService;
    }

    public async Task ProcessAsset(Guid assetId)
    {
        var asset = await _uow.GenericRepository.GetByIdAsync<Asset>(assetId);

        if (asset is null)
        {
            _logger.LogWarning("Asset with id {AssetId} was not found. Skipping processing.", assetId);
            return;
        }

        var mediaUrl = await _storageService.GetUrl(asset.FileName, asset.UserId);

        if (string.IsNullOrEmpty(mediaUrl))
        {
            _logger.LogError("The media url is empty. Skipping processing.");
            return;
        }

        var assetDescribed = await _llmService.DescribeVideo(mediaUrl);

        _logger.LogInformation($"Video asset described: {assetDescribed}");

        var descriptionEmbedded = await _embeddingsHandlerService.GenerateEmbedding(assetDescribed, asset.Label ?? "");

        await _vectorDatabaseService.InsertEmbedding(asset.Id, descriptionEmbedded, new()
        {
            { "text", assetDescribed },
            { "label", asset.Label ?? "" },
            { "asset_id", asset.Id.ToString() },
            { "user_id", asset.UserId.ToString() }
        }, asset.UserId);

        asset.ProcessAsset();
        
        _uow.GenericRepository.Update(asset);
        await _uow.CommitAsync();
    }
}