
using PersonalKnowledge.Application.Requests;
using PersonalKnowledge.Application.Responses;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public interface IMessageService
{
    public Task<ChatResponse> SendMessage(SendMessageRequest request);
    public Task<ChatResponse> GenerateMessageByText(string text, Guid userId);
}

public class MessageService(
    IUnitOfWork uow, 
    ITokenService tokenService, 
    IEmbeddingsHandlerService embeddingsHandlerService,
    IVectorDatabaseService vectorDatabaseService,
    ILLMService llmService) : IMessageService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmbeddingsHandlerService _embeddingsHandlerService = embeddingsHandlerService;
    private readonly IVectorDatabaseService _vectorDatabaseService = vectorDatabaseService;
    private readonly ILLMService _llmService = llmService;
    
    public async Task<ChatResponse> SendMessage(SendMessageRequest request)
    {
        var user = await _tokenService.GetUserByTokenAsync();
        if (user == null)
        {
            return new ChatResponse { Message = "Unauthorized", SentAt = DateTime.UtcNow };
        }

        var responseMessage = await _llmService.ProcessText(request.Message, user.Id);

        return new ChatResponse
        {
            Message = responseMessage,
            SentAt = DateTime.UtcNow
        };
    }

    public async Task<ChatResponse> GenerateMessageByText(string text, Guid userId)
    {
        var response = await _llmService.ProcessText(text, userId);
        
        return new ChatResponse
        {
            Message = response,
            SentAt = DateTime.UtcNow
        };       
    }
}