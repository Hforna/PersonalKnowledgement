using Hangfire;
using Microsoft.Extensions.Logging;
using PersonalKnowledge.Application.Dtos;
using PersonalKnowledge.Domain.Constants;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public interface IReceiverService
{
    public Task Receive(ReceiveDto receiveDto, ConversationSource source);
}

public class ReceiverService(IUnitOfWork uow, ILogger<IReceiverService> logger, 
    IFileHandlerService fileHandlerService, ILLMService llmService, IMessageService messageService, IAssetSenderService senderService, 
    IEmbeddingsHandlerService embeddingsHandlerService, IVectorDatabaseService vectorDatabaseService) : IReceiverService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<IReceiverService> _logger = logger;
    private readonly IFileHandlerService _fileHandlerService = fileHandlerService;
    private readonly ILLMService _llmService = llmService;
    private readonly IMessageService _messageService = messageService;
    private readonly IAssetSenderService _senderService = senderService;
    private readonly IEmbeddingsHandlerService _embeddingsHandlerService = embeddingsHandlerService;
    private readonly IVectorDatabaseService _vectorDatabase = vectorDatabaseService;
    
    public async Task Receive(ReceiveDto receiveDto, ConversationSource source)
    {
        var user = await _uow.UserRepository.GetUserByPhone(receiveDto.From);

        if (user is null)
        {
            _logger.LogError("User with phone {Phone} sent from receiver not found", receiveDto.From);
        }
        
        if (receiveDto.MediaReceivedDtos.Count > 0)
        {
            foreach (var mediaDto in receiveDto.MediaReceivedDtos)
            {
                var fileExtension = _fileHandlerService.GetFileExtension(mediaDto.MediaType);
                
                if (fileExtension is null)
                {
                    _logger.LogWarning("Could not identify file extension for media type {MediaType}", mediaDto.MediaType);
                    continue;
                }
                
                var mediaType = FileTypeIdentifiers.GetMediaType(fileExtension.Value);

                var mediaAsset = new Asset()
                {
                    MediaType = mediaType,
                    UserId = user.Id,
                    FileName = mediaDto.MediaUrl,
                    FileType = fileExtension.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Label = receiveDto.Body
                };

                await _uow.GenericRepository.AddAsync(mediaAsset);
                await _uow.CommitAsync();
                
                switch (mediaType)
                {
                    case MediaType.DOCUMENT:
                        BackgroundJob.Enqueue<ITextAssetProcessor>(d => d.ProcessAsset(mediaAsset.Id));
                        break;
                    case MediaType.IMAGE:
                        BackgroundJob.Enqueue<IImageAssetProcessor>(m => m.ProcessAsset(mediaAsset.Id));
                        break;
                    case MediaType.VIDEO:
                        BackgroundJob.Enqueue<IVideoAssetProcessor>(m => m.ProcessAsset(mediaAsset.Id));
                        break;
                    case MediaType.AUDIO:
                        BackgroundJob.Enqueue<IAudioAssetProcessor>(m => m.ProcessAsset(mediaAsset.Id));
                        break;
                }
            }
        }

        if (!string.IsNullOrEmpty(receiveDto.Body))
        {
            BackgroundJob.Enqueue<IMessageProcessor>(d => d.ProcessMessage(receiveDto, user.Id, source));
        }
    }
}