namespace PersonalKnowledge.Application.Responses;

public class AuthResponse
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
