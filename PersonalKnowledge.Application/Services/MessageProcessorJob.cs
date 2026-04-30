using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public class MessageProcessorJob(
    IUnitOfWork uow,
    ILLMService llmService,
    IMessageService messageService,
    IAssetSenderService senderService,
    ILogger<MessageProcessorJob> logger) : IMessageProcessor
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILLMService _llmService = llmService;
    private readonly IMessageService _messageService = messageService;
    private readonly IAssetSenderService _senderService = senderService;
    private readonly ILogger<MessageProcessorJob> _logger = logger;

    public async Task ProcessMessage(ReceiveDto receiveDto, Guid userId)
    {
        _logger.LogInformation("Processing message from user {UserId}: {Body}", userId, receiveDto.Body);

        var isQuestion = await _llmService.IsTextQuestion(receiveDto.Body);

        var conversation = await GetOrCreateConversation(userId);

        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = receiveDto.Body,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.GenericRepository.AddAsync(userMessage);
        await _uow.CommitAsync();

        if (isQuestion)
        {
            var response = await _messageService.GenerateMessageByText(receiveDto.Body, userId);

            var assistantMessage = new Message
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = response.Message,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _uow.GenericRepository.AddAsync(assistantMessage);
            await _uow.CommitAsync();

            var responseDto = new ChatResponseToSenderDto { Message = response.Message, Phone = receiveDto.From };
            await _senderService.Send(responseDto);
        }
    }

    private async Task<Conversation> GetOrCreateConversation(Guid userId)
    {
        var conversations = await _uow.GenericRepository.GetAllAsync<Conversation>();
        var conversation = conversations
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefault(c => c.UserId == userId && c.Title == "WhatsApp Conversation");

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Title = "WhatsApp Conversation",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _uow.GenericRepository.AddAsync(conversation);
            await _uow.CommitAsync();
        }

        return conversation;
    }
}
