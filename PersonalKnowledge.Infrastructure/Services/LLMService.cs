using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Audio;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;
using System.Net.Http;
using System.Text.Json;
using PersonalKnowledge.Application.Services;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Exceptions;
using PersonalKnowledge.Domain.Helpers;

namespace PersonalKnowledge.Infrastructure.Services;

public class LLMService : LLMToolsService, ILLMService
{
    private readonly OpenAIClient _openAiClient;
    private readonly OpenAiSettings _openAiSettings;
    private readonly ILogger<LLMService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    private readonly IEmbeddingsHandlerService _embeddingsHandlerService;
    private readonly IVectorDatabaseService _vectorDatabaseService;
    
    public LLMService(
        OpenAIClient openAiClient,
        OpenAiSettings openAiSettings,
        ILogger<LLMService> logger,
        IHttpClientFactory httpClientFactory,
        ISpotifyService spotifyService,
        IUnitOfWork uow,
        IToolsService toolsService,
        IAssetSenderService assetSenderService,
        IWhatsAppSender whatsAppSender,
        IEmbeddingsHandlerService embeddingsHandlerService,
        IVectorDatabaseService vectorDatabaseService)
        : base(spotifyService, logger, httpClientFactory, uow, toolsService, assetSenderService, whatsAppSender, embeddingsHandlerService, vectorDatabaseService)
    {
        _openAiClient = openAiClient;
        _openAiSettings = openAiSettings;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _embeddingsHandlerService = embeddingsHandlerService;
        _vectorDatabaseService = vectorDatabaseService;
    }

    private void AddToolsToChat(ChatCompletionOptions options)
    {
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
                    artistName = new { type = "string", description = "The name of the artist of the song." },
                },
                required = new[] { "playlistName", "songName" }
            })
        ));
        
        options.Tools.Add(ChatTool.CreateFunctionTool(
            "ReturnSpotifyOAuthUrl",
            "Send a url to user link their spotify account to application"
            ));
        
        options.Tools.Add(ChatTool.CreateFunctionTool(
            "SendMessageToUser", 
            "If none function being called a message must be sent to the user for answering user prompt or sending a message to the user if needed after executing all the functions",
            functionParameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    body = new { type = "string", description = "the text generated that will be sent to the user" }
                },
                required = new [] { "body" }
            })));

        options.Tools.Add(ChatTool.CreateFunctionTool(
            "GetContextByEmbeddings",
            "Search for relevant information/context from the user's personal knowledge base using a query string. Use this when you need more information to answer the user's question.",
            functionParameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string", description = "The search query to find relevant information." }
                },
                required = new[] { "query" }
            })
        ));
    }
    
    public async Task<string> ProcessText(string prompt, Guid userId)
    {
        var modelOrDeployment = _openAiSettings.IsAzureOpenAI 
            ? _openAiSettings.ChatDeploymentName 
            : _openAiSettings.ChatModel;

        var client = _openAiClient.GetChatClient(modelOrDeployment);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an expert personal assistant. You have access to the user's personal knowledge base. If you need information to answer a question, use the 'GetContextByEmbeddings' tool. If after searching you don't know the answer, just say that you don't know, don't try to make up an answer. Keep the answer concise and strictly relevant. Respond in the language used by the user."),
            new UserChatMessage(prompt)
        };

        var options = new ChatCompletionOptions();
        AddToolsToChat(options);

        var requiresAction = false;
        var lastAssistantMessage = string.Empty;

        do
        {
            var response = await client.CompleteChatAsync(messages, options);

            switch (response.Value.FinishReason)
            {
                case ChatFinishReason.Stop:
                    requiresAction = false;
                    lastAssistantMessage = response.Value.Content[0].Text;
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

        return lastAssistantMessage;
    }

    private async Task<string> ExecuteTools(ChatToolCall call, Guid userId)
    {
        var functionName = call.FunctionName;
        
        var methods = new Dictionary<string, Func<Task<string>>>
        {
            ["AddToSpotifyPlaylist"] = () => AddMusicToPlaylist(call, userId),
            ["ReturnSpotifyOAuthUrl"] = () => ReturnSpotifyOAuthUrl(userId),
            ["SendMessageToUser"] = () => SendMessageToUser(call, userId),
            ["GetContextByEmbeddings"] = () => GetContextByEmbeddings(call, userId)
        };
        
        if (methods.TryGetValue(functionName, out Func<Task<string>> method))
            return await method();

        return "The requested resource was not found";
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

public class LLMToolsService
{
    private readonly ISpotifyService _spotifyService;
    private readonly ILogger<LLMService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWork _uow;
    private readonly IToolsService _toolsService;
    private readonly IAssetSenderService _assetSenderService;
    private readonly IWhatsAppSender _whatsAppSender;
    protected readonly IEmbeddingsHandlerService _embeddingsHandlerService;
    protected readonly IVectorDatabaseService _vectorDatabaseService;
    
    protected LLMToolsService(ISpotifyService spotifyService, ILogger<LLMService> logger, 
        IHttpClientFactory httpClientFactory, IUnitOfWork uow, IToolsService toolsService, IAssetSenderService assetSenderService, IWhatsAppSender whatsAppSender,
        IEmbeddingsHandlerService embeddingsHandlerService, IVectorDatabaseService vectorDatabaseService)
    {
        _spotifyService = spotifyService;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _uow = uow;
        _whatsAppSender = whatsAppSender;
        _assetSenderService = assetSenderService;
        _toolsService = toolsService;
        _embeddingsHandlerService = embeddingsHandlerService;
        _vectorDatabaseService = vectorDatabaseService;
    }

    protected async Task<string> SendMessageToUser(ChatToolCall call, Guid userId)
    {
        using var document = JsonDocument.Parse(call.FunctionArguments);
        var body = document.RootElement.GetProperty("body").GetString();

        var user = await _uow.UserRepository.UserById(userId);

        await _assetSenderService.Send(new ChatResponseToSenderDto() { Message = body, Phone = PhoneHelper.NormalizePhoneNumber(user.PhoneNumber)});

        return "message sent successfully";
    }

    protected async Task<string> AddMusicToPlaylist(ChatToolCall call, Guid userId)
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

    protected async Task<string> ReturnSpotifyOAuthUrl(Guid userId)
    {
        var user = await _uow.UserRepository.UserById(userId)
                   ?? throw new EntityNotFoundException("The user was not found");

        var normalizedPhone = PhoneHelper.NormalizePhoneNumber(user.PhoneNumber);
        await _whatsAppSender.SendLinkWithButton(normalizedPhone, "Connect to Spotify", normalizedPhone);

        return "Message sent successfully";
    }

    protected async Task<string> GetContextByEmbeddings(ChatToolCall call, Guid userId)
    {
        using var document = JsonDocument.Parse(call.FunctionArguments);
        var query = document.RootElement.GetProperty("query").GetString();

        if (string.IsNullOrEmpty(query))
            return "Query is empty";

        var queryEmbeddings = await _embeddingsHandlerService.GenerateEmbedding(query);
        var results = await _vectorDatabaseService.SearchSimilar(queryEmbeddings, userId);

        if (results == null || results.Count == 0)
            return "No relevant information found in the knowledge base.";

        return string.Join("\n", results.Select(r => $"[Asset Label: {r.label}] Content: {r.text}"));
    }
}