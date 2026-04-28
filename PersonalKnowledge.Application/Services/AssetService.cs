using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PersonalKnowledge.Application.Requests;
using PersonalKnowledge.Domain.Constants;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Exceptions;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public interface IAssetService
{
    public Task UploadAsset(UploadAssetRequest request); 
}

public class AssetService : IAssetService
{
    private readonly IFileHandlerService _fileHandlerService;
    private readonly IEnumerable<IAssetHandlerService> _assetHandlerServices;
    private readonly ILogger<AssetService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IStorageService _storageService;
    
    public AssetService(IFileHandlerService fileHandlerService, IEnumerable<IAssetHandlerService> parserService,
        ILogger<AssetService> logger, IUnitOfWork unitOfWork, ITokenService tokenService, IStorageService storageService)
    {
        _fileHandlerService = fileHandlerService;
        _assetHandlerServices = parserService;
        _logger = logger;
        _storageService = storageService;
        _tokenService = tokenService;
        _uow = unitOfWork;       
    }

    public async Task UploadAsset(UploadAssetRequest request)
    {
        var user = await _tokenService.GetUserByTokenAsync();
            
        await _uow.BeginTransactionAsync();

        using var assetStream = request.File.OpenReadStream();
        var fileName = request.File.FileName;
        
        var (isValid, extension) = _fileHandlerService.IsValidFile(assetStream, fileName);
        
        if(extension is null || !isValid)
            throw new InvalidAssetFormatException(fileName);
        
        _logger.LogInformation($"Asset extension: {extension}");

        var mediaType = FileTypeIdentifiers.GetMediaType(extension.Value);

        var asset = new Asset()
        {
            FileName = fileName,
            FileType = extension.Value,
            UserId = user.Id,
            Label = request.Label,
            MediaType = mediaType
        };

        if (mediaType == MediaType.DOCUMENT)
        {
            var assetHandlerService = _assetHandlerServices.FirstOrDefault(d => d.AssetParsingType == extension)
                ?? throw new AssetHandlerNotFoundException(extension.Value);

            var assetText = await assetHandlerService.GetAssetText(assetStream);
            var textChunks = _fileHandlerService.Chunk(assetText, 20, 30);
            asset.TotalChunks = textChunks.Length;

            await _uow.GenericRepository.AddAsync(asset);

            var chunkIndex = 0;
            foreach (var textChunk in textChunks)
            {
                var chunk = new Chunk()
                {
                    Text = textChunk,
                    ChunkIndex = chunkIndex++,
                    AssetId = asset.Id,
                    Asset = asset
                };
                await _uow.GenericRepository.AddAsync(chunk);
            }
        }
        else
        {
            await _uow.GenericRepository.AddAsync(asset);
        }

        assetStream.Position = 0;
        var storageKey = await _storageService.UploadAsync(assetStream, fileName);
        asset.FileName = storageKey;
        
        await _uow.CommitAsync();
        
        switch (asset.MediaType)
        {
            case MediaType.DOCUMENT:
                BackgroundJob.Enqueue<ITextAssetProcessor>(d => d.ProcessAsset(asset.Id));
                break;
            case MediaType.IMAGE:
                BackgroundJob.Enqueue<IImageAssetProcessor>(m => m.ProcessAsset(asset.Id));
                break;
            case MediaType.VIDEO:
                BackgroundJob.Enqueue<IVideoAssetProcessor>(m => m.ProcessAsset(asset.Id));
                break;
            case MediaType.AUDIO:
                BackgroundJob.Enqueue<IAudioAssetProcessor>(m => m.ProcessAsset(asset.Id));
                break;
        }
    }
}