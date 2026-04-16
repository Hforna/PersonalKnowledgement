using PersonalKnowledge.Domain.Entities;

namespace PersonalKnowledge.Domain.Services;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string userName, IEnumerable<string> roles);
    Task<User?> GetUserByTokenAsync();
}
