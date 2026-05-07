using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Audio;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;
using System.Net.Http;
using System.Text.Json;

namespace PersonalKnowledge.Infrastructure.Services;

public class LLMService : ILLMService
{
    private readonly OpenAIClient _openAiClient;
    private readonly OpenAiSettings _openAiSettings;
    private readonly ISpotifyService _spotifyService;
    private readonly ILogger<LLMService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWork _uow;
    
    public LLMService(OpenAIClient openAiClient, OpenAiSettings openAiSettings, ILogger<LLMService> logger, IHttpClientFactory httpClientFactory, ISpotifyService spotifyService, IUnitOfWork uow)
    {
        _openAiClient = openAiClient;
        _openAiSettings = openAiSettings;
        _logger = logger;       
        _httpClientFactory = httpClientFactory;
        _spotifyService = spotifyService;
        _uow = uow;
    }
    
    public async Task<string> GenerateResponseByContext(string context, string prompt, Guid userId)
    {
        var messages = await ProcessText(context, prompt, userId);
        return messages.Last().Content[0].Text ?? "I'm sorry, the response was empty.";
    }

    public async Task<List<ChatMessage>> ProcessText(string context, string prompt, Guid userId)
    {
        var modelOrDeployment = _openAiSettings.IsAzureOpenAI 
            ? _openAiSettings.ChatDeploymentName 
            : _openAiSettings.ChatModel;

        var client = _openAiClient.GetChatClient(modelOrDeployment);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an expert assistant. Use the following context to answer the question at the end. If you don't know the answer, just say that you don't know, don't try to make up an answer. Keep the answer concise and strictly relevant to the provided context. Remember to return the response in the language that was found in the context."),
            new UserChatMessage($"""
                                 Context:
                                 {context}

                                 User Question:
                                 {prompt}
                                 """)
        };

        var options = new ChatCompletionOptions();
        options.Tools.Add(ChatTool.CreateFunctionTool(
                "AddToSpotifyPlaylist",
                "Add a specific music to a determined spotify playlist.",
                functionParameters: BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        playlistName = new { type = "string", description = "The name of the playlist to add the song to." },
                        songName = new { type = "string", description = "The name of the song to add to the playlist." },
                        artistName = new { type = "string", description = "The name of the artist of the song." }
                    },
                    required = new[] { "playlistName", "songName" }
                })
            ));

        var requiresAction = false;

        do
        {
            var response = await client.CompleteChatAsync(messages, options);

            switch (response.Value.FinishReason)
            {
                case ChatFinishReason.Stop:
                    requiresAction = false;
                    messages.Add(new AssistantChatMessage(response.Value));
                    break;
                case ChatFinishReason.ToolCalls:
                    messages.Add(new AssistantChatMessage(response.Value));

                    foreach (var toolCall in response.Value.ToolCalls)
                    {
                        var toolMessage = await ExecuteTools(toolCall, userId);
                        messages.Add(new ToolChatMessage(toolCall.Id, toolMessage));
                    }
                    
                    requiresAction = true;
                    break;
            }
        } while (requiresAction);

        return messages;
    }

    private async Task<string> ExecuteTools(ChatToolCall call, Guid userId)
    {
        if (call.FunctionName.Equals("AddToSpotifyPlaylist", StringComparison.OrdinalIgnoreCase))
        {
            using var jsonDocument = JsonDocument.Parse(call.FunctionArguments);
            var playlistName = jsonDocument.RootElement.TryGetProperty("playlistName", out var playlistNameElement);
            var songName = jsonDocument.RootElement.TryGetProperty("songName", out var songNameElement);
            var artistName = jsonDocument.RootElement.TryGetProperty("artistName", out var artistNameElement);
            var playlistNameString = playlistNameElement.GetString();
            var songNameString = songNameElement.GetString();
            var artistNameString = !artistName ? "" : artistNameElement.GetString();

            var tool = await _uow.ToolsRepository.GetUserToolAsync(userId, ToolType.Spotify);
            if (tool == null || string.IsNullOrEmpty(tool.AccessToken))
            {
                return "Spotify tool not configured or access token missing.";
            }

            var spotifyUserId = tool.ToolAccountId; //tool.ToolAccountId;
            var accessToken = tool.AccessToken;
            
            var selectedPlaylist = await _spotifyService.GetPlaylistIdByName(playlistNameString, spotifyUserId, accessToken);

            if (string.IsNullOrEmpty(selectedPlaylist))
                return "Playlist provided not found by api";

            var addMusic =
               await _spotifyService.AddMusicToPlaylist(selectedPlaylist, accessToken, songNameString, artistNameString);

            return addMusic;
        }
        
        return string.Empty;
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

    public async Task<string> DescribeVideo(string videoUrl)
    {
        // Placeholder implementation for video description
        return await Task.FromResult($"Video content at {videoUrl}");
    }

    public async Task<string> ProcessAudio(string audioUrl)
    {
        try
        {
            var modelOrDeployment = _openAiSettings.IsAzureOpenAI 
                ? _openAiSettings.ChatDeploymentName 
                : "whisper-1"; // Whisper model name

            var client = _openAiClient.GetAudioClient(modelOrDeployment);

            Stream audioStream;
            string fileName;

            if (audioUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var httpClient = _httpClientFactory.CreateClient();
                audioStream = await httpClient.GetStreamAsync(audioUrl);
                fileName = Path.GetFileName(new Uri(audioUrl).LocalPath);
                if (string.IsNullOrEmpty(fileName)) fileName = "audio.mp3";
            }
            else if (audioUrl.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                var filePath = new Uri(audioUrl).LocalPath;
                audioStream = File.OpenRead(filePath);
                fileName = Path.GetFileName(filePath);
            }
            else
            {
                audioStream = File.OpenRead(audioUrl);
                fileName = Path.GetFileName(audioUrl);
            }

            using (audioStream)
            {
                var response = await client.TranscribeAudioAsync(audioStream, fileName);
                return response.Value.Text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio from {AudioUrl}", audioUrl);
            return $"Error transcribing audio: {ex.Message}";
        }
    }

    public async Task<bool> IsTextQuestion(string text)
    {
        var modelOrDeployment = _openAiSettings.IsAzureOpenAI 
            ? _openAiSettings.ChatDeploymentName 
            : _openAiSettings.ChatModel;
        
        var client = _openAiClient.GetChatClient(modelOrDeployment);

        var chatMessage = new ChatMessage[]
        {
            new SystemChatMessage("Determine if the user's input is a question. Respond ONLY with 'true' if it is a question or 'false' if it is not. A question may be identified by its structure (e.g., starting with 'what', 'how', 'who', 'is', 'can', 'quais', 'como', 'onde', 'quem', etc.) or by an interrogation mark. Even without a question mark, if it sounds like a request for information, return 'true'."),
            new UserChatMessage(text)
        };

        var response = await client.CompleteChatAsync(chatMessage);

        var responseText = response.Value.Content[0].Text?.Trim().ToLower() ?? string.Empty;
        
        _logger.LogInformation($"LLM Raw response for IsTextQuestion: '{responseText}'");

        var isQuestion = responseText.Contains("true");
        
        _logger.LogInformation($"Is question result: {isQuestion}");

        return isQuestion;
    }
}