using Azure.AI.OpenAI;
using System.ClientModel;
using PersonalKnowledge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Infrastructure.Services;
using Azure.Storage.Blobs;
using OpenAI;
using Qdrant.Client;

using PersonalKnowledge.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using Twilio;

namespace PersonalKnowledge.Infrastructure;

public static class DependenciesConfiguration
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

        // JWT Authentication Configuration
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestService, RequestService>();

        services.AddDbContext<DataContext>(d => d.UseSqlServer(configuration.GetConnectionString("sqlserver")));

        services.AddScoped<ITextAssetProcessor, TextAssetProcessorJob>();
        services.AddScoped<IImageAssetProcessor, ImageAssetProcessorJob>();
        services.AddScoped<IVideoAssetProcessor, VideoAssetProcessorJob>();
        services.AddScoped<IAudioAssetProcessor, AudioAssetProcessorJob>();
        
        var storageType = configuration.GetValue<string>("Storage:Type") ?? "Local";

        if (storageType.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            var azureConnectionString = configuration.GetConnectionString("AzureBlobStorage");
            
            if (string.IsNullOrEmpty(azureConnectionString))
                throw new InvalidOperationException("AzureBlobStorage connection string is not configured");

            var accountName = configuration["AzureBlobStorage:AccountName"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];

            var uri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}");

            var credential = new Azure.Storage.StorageSharedKeyCredential(
                accountName,
                configuration["AzureBlobStorage:AccountKey"]
            );

            var blobContainerClient = new BlobContainerClient(uri, credential);
            
            services.AddScoped(_ => blobContainerClient);
            services.AddScoped<IStorageService, AzureBlobStorageService>();
        }
        else
        {
            var storagePath = configuration.GetValue<string>("Storage:LocalPath") ?? Path.Combine(AppContext.BaseDirectory, "uploads");
            services.AddScoped<IStorageService>(_ => new LocalFileStorageService(storagePath));
        }
        
        var qdrantUrl = configuration.GetConnectionString("qdrant");
        if (string.IsNullOrEmpty(qdrantUrl))
            throw new InvalidOperationException("Qdrant connection string is not configured");

        services.AddSingleton<IQdrantClient>(new QdrantClient(new Uri(qdrantUrl)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGenericRepository, GenericRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        TwilioClient.Init(configuration.GetValue<string>("services:twilio:AccountSid"), configuration.GetValue<string>("services:twilio:AuthToken"));

        services.AddScoped<IAssetSenderService, WhatsAppSenderService>(d => 
            new WhatsAppSenderService(d.CreateScope().ServiceProvider.GetRequiredService<ILogger<WhatsAppSenderService>>(), 
            configuration.GetValue<string>("services:twilio:PhoneSender")));

        var openAiSettings = new OpenAiSettings
        {
            ApiKey = configuration.GetValue<string>("services:openai:ApiKey") ?? throw new InvalidOperationException("OpenAI ApiKey is not configured"),
            ChatModel = configuration.GetValue("services:openai:ChatModel", "gpt-4o"),
            ChatDeploymentName = configuration.GetValue("services:openai:ChatDeploymentName", "gpt-4o"),
            EmbeddingModel = configuration.GetValue("services:openai:EmbeddingModel", "text-embedding-3-small"),
            EmbeddingDeploymentName = configuration.GetValue("services:openai:EmbeddingDeploymentName", "text-embedding-3-small"),
            EmbeddingSize = configuration.GetValue("services:openai:EmbeddingSize", 1536),
            Endpoint = configuration.GetValue("services:openai:Endpoint", "https://api.openai.com/v1"),
            IsAzureOpenAI = configuration.GetValue("services:openai:IsAzureOpenAI", false)
        };

        services.AddSingleton(openAiSettings);
        
        services.AddScoped<OpenAIClient>(sp =>
        {
            var apiKey = new ApiKeyCredential(openAiSettings.ApiKey);

            if (openAiSettings.IsAzureOpenAI)
            {
                var options = new AzureOpenAIClientOptions();
                var endpoint = openAiSettings.Endpoint;
                if (!endpoint.Contains("/openai/deployments")) 
                {
                   // The SDK handles the path, we just need the base endpoint
                }
                var client = new AzureOpenAIClient(new Uri(endpoint), apiKey, options);
                Console.WriteLine($"[DEBUG_LOG] Initialized AzureOpenAIClient with Endpoint: {endpoint}");
                return client;
            }
            else
            {
                var options = new OpenAIClientOptions()
                {
                    Endpoint = new Uri(openAiSettings.Endpoint)
                };
                return new OpenAIClient(apiKey, options);
            }
        });

        services.AddScoped<IVectorDatabaseService, QdrantService>();
        services.AddScoped<IEmbeddingsHandlerService, EmbeddingsHandlerService>();
        services.AddScoped<ILLMService, LLMService>();
        services.AddHttpClient();
    }
}

public class OpenAiSettings
{
    public required string ApiKey { get; set; }
    public string ChatModel { get; set; } = "gpt-4o";
    public string ChatDeploymentName { get; set; } = "gpt-4o";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-3-small";
    public int EmbeddingSize { get; set; } = 1536;
    public string Endpoint { get; set; }
    public bool IsAzureOpenAI { get; set; }
}
