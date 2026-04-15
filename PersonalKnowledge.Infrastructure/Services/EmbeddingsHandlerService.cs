using OpenAI;
using PersonalKnowledge.Domain.Services;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;

namespace PersonalKnowledge.Infrastructure.Services;

public class EmbeddingsHandlerService : IEmbeddingsHandlerService
{
    private readonly OpenAIClient _openAiClient;
    private readonly OpenAiSettings _openAiSettings;
    private readonly ILogger<EmbeddingsHandlerService> _logger;

    public EmbeddingsHandlerService(OpenAIClient openAiClient, OpenAiSettings openAiSettings, ILogger<EmbeddingsHandlerService> logger)
    {
        _openAiClient = openAiClient;
        _openAiSettings = openAiSettings;
        _logger = logger;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbedding(string chunk)
    {
        try
        {
            if (_openAiSettings == null)
            {
                _logger.LogError("_openAiSettings is NULL in GenerateEmbedding!");
                throw new InvalidOperationException("_openAiSettings is null");
            }

            var modelOrDeployment = _openAiSettings.IsAzureOpenAI 
                ? _openAiSettings.EmbeddingDeploymentName 
                : _openAiSettings.EmbeddingModel;

            _logger.LogInformation($"[DEBUG_LOG] Generating embedding. Model/Deployment: {modelOrDeployment}. Azure: {_openAiSettings.IsAzureOpenAI}. Endpoint: {_openAiSettings.Endpoint}");

            var client = _openAiClient.GetEmbeddingClient(modelOrDeployment);

            var result = await client.GenerateEmbeddingsAsync(new List<string>() {chunk});
            
            return result.Value.First().ToFloats();
        }
        catch (Exception e)
        {
            var modelOrDeployment = _openAiSettings?.IsAzureOpenAI == true 
                ? _openAiSettings?.EmbeddingDeploymentName 
                : _openAiSettings?.EmbeddingModel;

            _logger.LogError($"Error generating embedding: {e.Message}");
            if (modelOrDeployment != null)
                _logger.LogError($"Model/Deployment being used: {modelOrDeployment}");
            _logger.LogError($"Exception: {e}");
            throw;
        }
    }
}

