using PersonalKnowledge.Domain.Dtos;

namespace PersonalKnowledge.Domain.Services;

public interface ISpotifyAuthenticationService
{
    public string BuildAuthenticationUri(string redirectUri, string? state = null);
    public Task<BaseTokenResponseDto> GetAccessToken(string code, string redirect_uri, string? state = "");
    public Task<TokenResponseWithScopesDto> RefreshUserToken(string refreshToken);
}