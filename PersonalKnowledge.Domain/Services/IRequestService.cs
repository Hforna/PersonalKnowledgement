namespace PersonalKnowledge.Domain.Services;

public interface IRequestService
{
    string? GetToken();
    string GetBaseUrl();
}
