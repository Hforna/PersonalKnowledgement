using PersonalKnowledge.Domain.Enums;

namespace PersonalKnowledge.Domain.Entities;

public class Tools : Entity
{
    public Guid UserId { get; set; }
    public string ToolAccountId { get; set; }
    public ToolType Type { get; set; }
    public string? AccessToken { get; set; } 
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}