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

    public string GetBaseUrl()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null) return string.Empty;

        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}
