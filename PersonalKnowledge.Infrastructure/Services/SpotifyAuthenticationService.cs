using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PersonalKnowledge.Infrastructure.Services;

public class SpotifyAuthenticationService : ISpotifyAuthenticationService
{
    private readonly string _clientId;
    private readonly string _scopes;
    private readonly string _redirectUri;
    private readonly string _clientSecret;
    private readonly IHttpClientFactory _httpClient;

    public SpotifyAuthenticationService(IConfiguration configuration, IHttpClientFactory httpClient)
    {
        _clientId = configuration.GetValue<string>("services:tools:spotify:clientId");
        _clientSecret = configuration.GetValue<string>("services:tools:spotify:clientSecret");
        _redirectUri = configuration.GetValue<string>("services:tools:spotify:redirectUri");
        _scopes = configuration.GetValue<string>("services:tools:spotify:scopes");
        _httpClient = httpClient;
    }
    
    public string BuildAuthenticationUri(string? state = null)
    {
        var uri = 
            $"https://accounts.spotify.com/authorize?response_type=code&client_id={_clientId}&redirect_uri={_redirectUri}&scope={_scopes}";

        if (!string.IsNullOrEmpty(state))
            uri += $"&state={state}";

        return uri;
    }

    public async Task<BaseTokenResponseDto> GetAccessToken(string code, string redirect_uri, string? state = null)
    {
        using var client = _httpClient.CreateClient();

        var authorizationBytes = Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}");
        var bytesToBase64 = Convert.ToBase64String(authorizationBytes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", bytesToBase64);
        client.DefaultRequestHeaders.Add("Content_Type", "application/x-www-form-urlencoded");

        var values = new Dictionary<string, string>()
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirect_uri }
        };

        if (string.IsNullOrEmpty(state) == false)
            values["state"] = state;
        
        var content = new FormUrlEncodedContent(values);
        
        var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<BaseTokenResponseDto>(responseContent);
    }

    public async Task<TokenResponseWithScopesDto> RefreshUserToken(string refreshToken)
    {
        using var client = _httpClient.CreateClient();

        var authorizationBytes = Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}");
        var bytesToBase64 = Convert.ToBase64String(authorizationBytes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", bytesToBase64);
        client.DefaultRequestHeaders.Add("Content_Type", "application/x-www-form-urlencoded");

        var requestValues = new Dictionary<string, string>()
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };

        var request = new FormUrlEncodedContent(requestValues);

        var response = await client.PostAsync("https://accounts.spotify.com/api/token", request);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<TokenResponseWithScopesDto>(content);
    }
}