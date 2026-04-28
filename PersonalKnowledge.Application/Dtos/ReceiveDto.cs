namespace PersonalKnowledge.Application.Dtos;

public class ReceiveDto
{
    public string Body { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string MessageSid { get; set; } = string.Empty;
    public string AccountSid { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? Label { get; set; }
    public List<MediaReceivedDto> MediaReceivedDtos { get; set; }
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public class MediaReceivedDto
{
    public string MediaType { get; set; }
    public string MediaUrl { get; set; }   
}