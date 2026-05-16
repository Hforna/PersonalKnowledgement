using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public class MessageProcessorJob(
    IUnitOfWork uow,
    ILLMService llmService,
    IMessageService messageService,
    IAssetSenderService senderService,
    ILogger<MessageProcessorJob> logger,
    ISenderResolver resolver,
    IVectorDatabaseService vectorDatabaseService,
    IEmbeddingsHandlerService embeddingsHandlerService) : IMessageProcessor
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILLMService _llmService = llmService;
    private readonly IMessageService _messageService = messageService;
    private readonly IAssetSenderService _senderService = senderService;
    private readonly ILogger<MessageProcessorJob> _logger = logger;
    private readonly ISenderResolver _senderResolver = resolver;
    private readonly IEmbeddingsHandlerService _embeddingsHandlerService = embeddingsHandlerService;
    private readonly IVectorDatabaseService _vectorDatabaseService = vectorDatabaseService;

    public async Task ProcessMessage(ReceiveDto receiveDto, Guid userId, ConversationSource source)
    {
        _logger.LogInformation("Processing message from user {UserId}: {Body}", userId, receiveDto.Body);
        
        var conversation = await GetOrCreateConversation(userId, source);

        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = receiveDto.Body,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.GenericRepository.AddAsync(userMessage);
        await _uow.CommitAsync();

        var sendingResponse = await _senderResolver.ResolveMessageSending(receiveDto.Body, userId);

        var assistantMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = sendingResponse,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.GenericRepository.AddAsync(assistantMessage);
        await _uow.CommitAsync();

        var responseDto = new ChatResponseToSenderDto { Message = sendingResponse, Phone = receiveDto.From };
    }

    private async Task<Conversation> GetOrCreateConversation(Guid userId, ConversationSource source)
    {
        var conversations = await _uow.ConversationRepository.GetUserConversationBySource(userId, source);
        var conversation = conversations
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefault(c => c.UserId == userId);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Title = $"User {userId} from {source.ToString()} Conversation",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ConversationSource = source
            };
            await _uow.GenericRepository.AddAsync(conversation);
            await _uow.CommitAsync();
        }

        return conversation;
    }
}
