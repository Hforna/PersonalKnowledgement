
using PersonalKnowledge.Application.Requests;
using PersonalKnowledge.Application.Responses;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public interface IMessageService
{
    public Task<ChatResponse> SendMessage(SendMessageRequest request);
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

        var requestEmbedding = await _embeddingsHandlerService.GenerateEmbedding(request.Message);
        
        var searchResults = await _vectorDatabaseService.SearchSimilar(requestEmbedding, user.Id);

        var context = string.Join("\n", searchResults.Select(r => r.text));

        if (string.IsNullOrWhiteSpace(context))
        {
            return new ChatResponse
            {
                Message = "I don't have any documents that can help me answer that question. Please upload some relevant documents first.",
                SentAt = DateTime.UtcNow
            };
        }

        var responseMessage = await _llmService.GenerateResponseByContext(context, request.Message);

        return new ChatResponse
        {
            Message = responseMessage,
            SentAt = DateTime.UtcNow
        };
    }
}