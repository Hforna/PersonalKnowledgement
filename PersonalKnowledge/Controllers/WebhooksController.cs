using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Services;
using PersonalKnowledge.Domain.Dtos;

namespace PersonalKnowledge.Controllers;

[ApiController]
[Route("[controller]/")]
public class WebhooksController(IReceiverService receiverService) : ControllerBase
{
    private readonly IReceiverService _receiverService = receiverService;

    [HttpPost("twilio")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> TwilioWebhook([FromForm] TwilioWhatsAppWebhook request)
    {
        var form = Request.Form;

        var mediaDtos = new List<MediaReceivedDto>();
        
        for (var i = 0; i < request.NumMedia; i++)
        {
            mediaDtos.Add(new MediaReceivedDto()
            {
                MediaUrl = form[$"MediaUrl{i}"],
                MediaType = form[$"MediaContentType{i}"]
            });
        }
        
        var receiveDto = new ReceiveDto
        {
            Body = request.Body ?? string.Empty,
            From = request.GetCleanedFrom(),
            To = request.GetCleanedTo(),
            MessageSid = request.MessageSid ?? string.Empty,
            AccountSid = request.AccountSid ?? string.Empty,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address,
            Label = request.Label,
            MediaReceivedDtos = mediaDtos,
            Metadata = new Dictionary<string, string>
            {
                { "SmsSid", request.SmsSid ?? string.Empty },
                { "FromCity", request.FromCity ?? string.Empty },
                { "FromState", request.FromState ?? string.Empty },
                { "FromZip", request.FromZip ?? string.Empty },
                { "FromCountry", request.FromCountry ?? string.Empty },
                { "ToCity", request.ToCity ?? string.Empty },
                { "ToState", request.ToState ?? string.Empty },
                { "ToZip", request.ToZip ?? string.Empty },
                { "ToCountry", request.ToCountry ?? string.Empty }
            }
        };

        await _receiverService.Receive(receiveDto);

        return Ok();
    }
}