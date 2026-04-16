using Microsoft.Extensions.DependencyInjection;
using PersonalKnowledge.Application.Services;

namespace PersonalKnowledge.Application;

public static class DependenciesConfiguration
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddSingleton<IFileHandlerService, FileHandlerService>();       
        services.AddScoped<IDocumentHandlerService, PdfHandlerService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
    }
}