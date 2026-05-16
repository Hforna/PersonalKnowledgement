using System.Text.Json;
using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Helpers;
using PersonalKnowledge.Domain.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace PersonalKnowledge.Infrastructure.Services;

public class WhatsAppSenderService(ILogger<WhatsAppSenderService> logger, string phoneSender) : IAssetSenderService, IWhatsAppSender
{
    public async Task Send(ChatResponseToSenderDto dto)
    {
        var from = $"whatsapp:{PhoneHelper.NormalizePhoneNumber(phoneSender)}";
        var to = $"whatsapp:{PhoneHelper.NormalizePhoneNumber(dto.Phone)}";

        var message = await MessageResource.CreateAsync(from: new PhoneNumber(from),
            to: new PhoneNumber(to),
            body: dto.Message,
            statusCallback: new Uri("https://24ec-2804-d51-4451-4600-b441-57c1-65e0-756b.ngrok-free.app/webhooks/twilio"));
        
        logger.LogInformation($"Message sent to: {message.To} with body: {message.Body}");       
    }

    public async Task SendLinkWithButton(string to, string label, string urlParam)
    {
        var token = Guid.NewGuid().ToString();

        var variables = new Dictionary<int, string>()
        {
            { 2, label },
            { 4, urlParam }
        };

        var from = $"whatsapp:{PhoneHelper.NormalizePhoneNumber(phoneSender)}";
        var toNormalized = $"whatsapp:{PhoneHelper.NormalizePhoneNumber(to)}";
        
        await MessageResource.CreateAsync(
            from: new PhoneNumber(from),
            to: new PhoneNumber(toNormalized),
            contentSid: "HX56823373e6d4b13305b52ada658878c6",
            contentVariables: JsonSerializer.Serialize(variables)
        );
    }
}