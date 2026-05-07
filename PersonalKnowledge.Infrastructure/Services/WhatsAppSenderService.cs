using System.Text.Json;
using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace PersonalKnowledge.Infrastructure.Services;

public class WhatsAppSenderService(ILogger<WhatsAppSenderService> logger, string phoneSender) : IAssetSenderService, IWhatsAppSender
{
    public async Task Send(ChatResponseToSenderDto dto)
    {
        var from = phoneSender.StartsWith("whatsapp:") ? phoneSender : $"whatsapp:{phoneSender}";
        var to = dto.Phone.StartsWith("whatsapp:") ? dto.Phone : $"whatsapp:{dto.Phone}";

        var message = await MessageResource.CreateAsync(from: new PhoneNumber(from),
            to: new PhoneNumber(to),
            body: dto.Message,
            statusCallback: new Uri("https://a886-201-54-159-16.ngrok-free.app/webhooks/twilio"));
        
        logger.LogInformation($"Message sent to: {message.To} with body: {message.Body}");       
    }

    public async Task SendLinkWithButton(string to, string text, string buttonText, string url)
    {
        var token = Guid.NewGuid().ToString();

        var contentVariables = new Dictionary<int, string>()
        {
            { 2, text },
            { 4, url }
        };

        await MessageResource.CreateAsync(
            from: new PhoneNumber(phoneSender),
            to: new PhoneNumber(to),
            contentSid: "HX8c48980108e176c47ee876dd12491c95",
            contentVariables: JsonSerializer.Serialize(contentVariables)
        );
    }
}