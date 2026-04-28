using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace PersonalKnowledge.Infrastructure.Services;

public class WhatsAppSenderService(ILogger<WhatsAppSenderService> logger, string phoneSender) : IAssetSenderService
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
}