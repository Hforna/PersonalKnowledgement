using Microsoft.AspNetCore.Identity;

namespace PersonalKnowledge.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
