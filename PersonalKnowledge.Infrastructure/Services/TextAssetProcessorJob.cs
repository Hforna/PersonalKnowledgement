using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Domain.Exceptions;
using Serilog;

namespace PersonalKnowledge.Infrastructure.Services;

public class TextAssetProcessorJob : ITextAssetProcessor
{
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    private readonly IEmbeddingsHandlerService _embeddingsHandlerService;
    private readonly IVectorDatabaseService _vectorDatabaseService;
    private readonly ILogger<TextAssetProcessorJob> _logger;

    public TextAssetProcessorJob(IUnitOfWork uow, IStorageService storageService, 
        IEmbeddingsHandlerService embeddingsHandlerService, IVectorDatabaseService vectorDatabaseService, ILogger<TextAssetProcessorJob> logger)
    {
        _uow = uow;
        _storageService = storageService;
        _embeddingsHandlerService = embeddingsHandlerService;
        _vectorDatabaseService = vectorDatabaseService;
        _logger = logger;       
    }

    public async Task ProcessAsset(Guid assetId)
    {
        var asset = await _uow.GenericRepository.GetByIdAsync<Asset>(assetId)
            ?? throw new EntityNotFoundException(nameof(Asset), assetId);

        var chunks = await _uow.AssetRepository.GetAssetChunksAsync(assetId);

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingsHandlerService.GenerateEmbedding(chunk.Text, asset.Label ?? "");
            
            _logger.LogInformation($"Embedding generated for chunk {chunk.Id}, {embedding}");

            await _vectorDatabaseService.InsertEmbedding(chunk.Id, embedding, new() 
            { 
                { "text", chunk.Text }, 
                { "label", asset.Label ?? "" },
                { "asset_id", asset.Id.ToString() },
                { "user_id", asset.UserId.ToString() }
            });
        }
        
        asset.ProcessAsset();
        
        _uow.GenericRepository.Update(asset);
        await _uow.CommitAsync();
    }
}