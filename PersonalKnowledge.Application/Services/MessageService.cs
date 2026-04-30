
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

        var requestEmbedding = await _embeddingsHandlerService.GenerateEmbedding(request.Message);
        
        var searchResults = await _vectorDatabaseService.SearchSimilar(requestEmbedding, user.Id);

        var assetIdsMissingLabels = searchResults
            .Where(r => string.IsNullOrWhiteSpace(r.label) && r.asset_id != Guid.Empty)
            .Select(r => r.asset_id)
            .Distinct()
            .ToList();

        if (assetIdsMissingLabels.Any())
        {
            var assets = await _uow.GenericRepository.GetByIdsAsync<Asset>(assetIdsMissingLabels);
            var assetInfo = assets.ToDictionary(a => a.Id, a => new { a.Label, a.FileName });

            foreach (var result in searchResults.Where(r => string.IsNullOrWhiteSpace(r.label)))
            {
                if (assetInfo.TryGetValue(result.asset_id, out var info))
                {
                    result.label = !string.IsNullOrWhiteSpace(info.Label) ? info.Label : info.FileName;
                }
            }
        }

        var context = string.Join("\n", searchResults.Select(r => $"[Asset Label: {r.label}] Content: {r.text}"));

        if (string.IsNullOrWhiteSpace(context))
        {
            return new ChatResponse
            {
                Message = "I don't have any documents that can help me answer that question. Please upload some relevant documents first.",
                SentAt = DateTime.UtcNow
            };
        }

        var responseMessage = await _llmService.GenerateResponseByContext($"{context}", request.Message, user.Id);

        return new ChatResponse
        {
            Message = responseMessage,
            SentAt = DateTime.UtcNow
        };
    }

    public async Task<ChatResponse> GenerateMessageByText(string text, Guid userId)
    {
        var embeddings = await _embeddingsHandlerService.GenerateEmbedding(text);

        var similarEmbeddings = await _vectorDatabaseService.SearchSimilar(embeddings, userId);
        
        var context = string.Join("\n", similarEmbeddings.Select(r => $"[Asset Label: {r.label}] Content: {r.text}"));

        if (string.IsNullOrWhiteSpace(context))
        {
            return new ChatResponse
            {
                Message = "I don't have any documents that can help me answer that question. Please upload some relevant documents first.",
                SentAt = DateTime.UtcNow
            };
        }
        
        var response = await _llmService.GenerateResponseByContext(context, text, userId);
        
        return new ChatResponse
        {
            Message = response,
            SentAt = DateTime.UtcNow
        };       
    }
}