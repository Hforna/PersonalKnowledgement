using OpenAI;
using OpenAI.Chat;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Services;

public class LLMService : ILLMService
{
    private readonly OpenAIClient _openAiClient;
    private readonly OpenAiSettings _openAiSettings;
    
    public LLMService(OpenAIClient openAiClient, OpenAiSettings openAiSettings)
    {
        _openAiClient = openAiClient;
        _openAiSettings = openAiSettings;
    }
    
    public async Task<string> GenerateResponseByContext(string context, string prompt)
    {
        var modelOrDeployment = _openAiSettings.IsAzureOpenAI 
            ? _openAiSettings.ChatDeploymentName 
            : _openAiSettings.ChatModel;

        Console.WriteLine($"[DEBUG_LOG] Getting ChatClient for {modelOrDeployment} (Azure: {_openAiSettings.IsAzureOpenAI})");
        var client = _openAiClient.GetChatClient(modelOrDeployment);

        if (client == null)
        {
            Console.WriteLine("[DEBUG_LOG] ChatClient is NULL");
            return "I'm sorry, the chat client could not be initialized.";
        }

        var messages = new ChatMessage[]
        {
            new SystemChatMessage("You are an expert assistant. Use the following context to answer the question at the end. If you don't know the answer, just say that you don't know, don't try to make up an answer. Keep the answer concise and strictly relevant to the provided context. Remember to return the response in the language that was found in the context."),
            new UserChatMessage($"""
                                 Context:
                                 {context}

                                 User Question:
                                 {prompt}
                                 """)
        };

        Console.WriteLine("[DEBUG_LOG] Calling CompleteChatAsync...");
        var response = await client.CompleteChatAsync(messages);

        if (response == null)
        {
            Console.WriteLine("[DEBUG_LOG] response is NULL");
            return "I'm sorry, the AI response was null.";
        }

        if (response.Value == null)
        {
            Console.WriteLine("[DEBUG_LOG] response.Value is NULL");
            return "I'm sorry, the AI response value was null.";
        }

        if (response.Value.Content == null || response.Value.Content.Count == 0)
        {
            Console.WriteLine("[DEBUG_LOG] response.Value.Content is NULL or EMPTY");
            return "I'm sorry, I couldn't generate a response.";
        }

        return response.Value.Content[0].Text ?? "I'm sorry, the response was empty.";
    }

    public async Task<string> DescribeImage(string imageUrl)
    {
        var modelOrDeployment = _openAiSettings.IsAzureOpenAI 
            ? _openAiSettings.ChatDeploymentName 
            : _openAiSettings.ChatModel;
        
        Console.WriteLine($"[DEBUG_LOG] Getting ChatClient for {modelOrDeployment} (Azure: {_openAiSettings.IsAzureOpenAI}) for image analysis");
        var client = _openAiClient.GetChatClient(modelOrDeployment);

        var messages = new ChatMessage[]
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart("Analyze the following image and describe its contents in detail, extracting any relevant information."),
                ChatMessageContentPart.CreateImagePart(new Uri(imageUrl))
            )
        };

        Console.WriteLine($"[DEBUG_LOG] Calling CompleteChatAsync for image: {imageUrl}");
        var response = await client.CompleteChatAsync(messages);

        if (response?.Value?.Content == null || response.Value.Content.Count == 0)
        {
            Console.WriteLine("[DEBUG_LOG] Image analysis response is NULL or EMPTY");
            return "I'm sorry, I couldn't analyze the image.";
        }
        
        return response.Value.Content[0].Text ?? "I'm sorry, the response was empty.";
    }
}