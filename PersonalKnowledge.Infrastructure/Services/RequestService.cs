using Microsoft.AspNetCore.Http;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Services;

public class RequestService(IHttpContextAccessor httpContextAccessor) : IRequestService
{
    public string? GetToken()
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader.Substring("Bearer ".Length).Trim();
    }
}
