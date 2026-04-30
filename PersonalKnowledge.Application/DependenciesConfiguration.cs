using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledge.Application.Services;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application;

public static class DependenciesConfiguration
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAssetService, AssetService>();
        services.AddSingleton<IFileHandlerService, FileHandlerService>();       
        services.AddScoped<IAssetHandlerService, PdfHandlerService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IReceiverService, ReceiverService>();
        services.AddScoped<IMessageProcessor, MessageProcessorJob>();
    }
}