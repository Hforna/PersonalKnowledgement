using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace PersonalKnowledge.Domain.Dtos;

/// <summary>
/// Represents all possible parameters sent by Twilio
/// to your webhook when a WhatsApp message is received.
/// </summary>
public class TwilioWhatsAppWebhook
{
    // ─── Core Parameters ──────────────────────────────────────────────────────

    [FromForm(Name = "MessageSid")]
    public string? MessageSid { get; set; }

    [FromForm(Name = "SmsSid")]
    public string? SmsSid { get; set; }

    [FromForm(Name = "SmsMessageSid")]
    public string? SmsMessageSid { get; set; }

    [FromForm(Name = "AccountSid")]
    public string? AccountSid { get; set; }

    [FromForm(Name = "MessagingServiceSid")]
    public string? MessagingServiceSid { get; set; }

    /// <summary>E.g. "whatsapp:+14017122661"</summary>
    [FromForm(Name = "From")]
    public string? From { get; set; }

    /// <summary>E.g. "whatsapp:+15558675310"</summary>
    [FromForm(Name = "To")]
    public string? To { get; set; }

    [FromForm(Name = "Body")]
    public string? Body { get; set; }

    [FromForm(Name = "NumMedia")]
    public int NumMedia { get; set; }

    [FromForm(Name = "NumSegments")]
    public int NumSegments { get; set; }

    // ─── WhatsApp-Specific Parameters ─────────────────────────────────────────

    [FromForm(Name = "ProfileName")]
    public string? ProfileName { get; set; }

    /// <summary>Sender's WhatsApp ID (phone number without +)</summary>
    [FromForm(Name = "WaId")]
    public string? WaId { get; set; }

    [FromForm(Name = "Forwarded")]
    public bool? Forwarded { get; set; }

    [FromForm(Name = "FrequentlyForwarded")]
    public bool? FrequentlyForwarded { get; set; }

    // ─── Geographic Parameters ─────────────────────────────────────────────────

    [FromForm(Name = "FromCity")]
    public string? FromCity { get; set; }

    [FromForm(Name = "FromState")]
    public string? FromState { get; set; }

    [FromForm(Name = "FromZip")]
    public string? FromZip { get; set; }

    [FromForm(Name = "FromCountry")]
    public string? FromCountry { get; set; }

    [FromForm(Name = "ToCity")]
    public string? ToCity { get; set; }

    [FromForm(Name = "ToState")]
    public string? ToState { get; set; }

    [FromForm(Name = "ToZip")]
    public string? ToZip { get; set; }

    [FromForm(Name = "ToCountry")]
    public string? ToCountry { get; set; }

    // ─── Location Sharing ──────────────────────────────────────────────────────

    [FromForm(Name = "Latitude")]
    public double? Latitude { get; set; }

    [FromForm(Name = "Longitude")]
    public double? Longitude { get; set; }

    [FromForm(Name = "Address")]
    public string? Address { get; set; }

    [FromForm(Name = "Label")]
    public string? Label { get; set; }

    // ─── Interactive / Button Parameters ──────────────────────────────────────

    [FromForm(Name = "ButtonPayload")]
    public string? ButtonPayload { get; set; }

    [FromForm(Name = "ButtonText")]
    public string? ButtonText { get; set; }

    /// <summary>"REPLY" or "ACTION"</summary>
    [FromForm(Name = "ButtonType")]
    public string? ButtonType { get; set; }

    /// <summary>Raw JSON string of the interactive flow response</summary>
    [FromForm(Name = "InteractiveData")]
    public string? InteractiveData { get; set; }

    /// <summary>Raw JSON string of the WhatsApp Flow completion payload</summary>
    [FromForm(Name = "FlowData")]
    public string? FlowData { get; set; }

    /// <summary>Raw JSON string with the full channel metadata</summary>
    [FromForm(Name = "ChannelMetadata")]
    public string? ChannelMetadata { get; set; }

    // ─── Click-to-WhatsApp Ad Parameters ──────────────────────────────────────

    [FromForm(Name = "ReferralBody")]
    public string? ReferralBody { get; set; }

    [FromForm(Name = "ReferralHeadline")]
    public string? ReferralHeadline { get; set; }

    [FromForm(Name = "ReferralSourceId")]
    public string? ReferralSourceId { get; set; }

    /// <summary>E.g. "post", "ad"</summary>
    [FromForm(Name = "ReferralSourceType")]
    public string? ReferralSourceType { get; set; }

    [FromForm(Name = "ReferralSourceUrl")]
    public string? ReferralSourceUrl { get; set; }

    [FromForm(Name = "ReferralMediaId")]
    public string? ReferralMediaId { get; set; }

    [FromForm(Name = "ReferralMediaContentType")]
    public string? ReferralMediaContentType { get; set; }

    [FromForm(Name = "ReferralMediaUrl")]
    public string? ReferralMediaUrl { get; set; }

    [FromForm(Name = "ReferralNumMedia")]
    public int? ReferralNumMedia { get; set; }

    [FromForm(Name = "ReferralCtwaClid")]
    public string? ReferralCtwaClid { get; set; }

    // ─── Reply-to-Message Parameters ──────────────────────────────────────────

    [FromForm(Name = "OriginalRepliedMessageSender")]
    public string? OriginalRepliedMessageSender { get; set; }

    [FromForm(Name = "OriginalRepliedMessageSid")]
    public string? OriginalRepliedMessageSid { get; set; }

    public string GetCleanedFrom() => From?.Replace("whatsapp:", "") ?? string.Empty;
    public string GetCleanedTo() => To?.Replace("whatsapp:", "") ?? string.Empty;
}

public class TwilioMedia
{
    public int    Index       { get; set; }
    public string? Url        { get; set; }
    public string? ContentType { get; set; }
}