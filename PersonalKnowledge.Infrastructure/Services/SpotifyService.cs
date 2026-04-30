using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Exceptions;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Services;

public class SpotifyService : ISpotifyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SpotifyService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly string _accessToken;

    public SpotifyService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<SpotifyService> logger, IUnitOfWork uow)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _uow = uow;
        _accessToken = configuration.GetValue<string>("services:tools:spotify:api_key");
    }

    public async Task<string> GetPlaylistIdByName(string playlistName, string spotifyUserId, string accessToken)
    {
        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://api.spotify.com/v1/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        int limit = 50;
        int offset = 0;
        bool hasMore = true;

        while (hasMore)
        {
            try
            {
                var response = await client.GetAsync($"users/{spotifyUserId}/playlists?limit={limit}&offset={offset}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error fetching playlists for user {UserId}: {StatusCode}", spotifyUserId, response.StatusCode);
                    break;
                }

                var data = await response.Content.ReadFromJsonAsync<SpotifyPaginationResponse<SpotifyItem>>();
                if (data?.Items == null || data.Items.Count == 0)
                {
                    break;
                }

                var playlist = data.Items.FirstOrDefault(p => p.Name.Equals(playlistName, StringComparison.OrdinalIgnoreCase));
                if (playlist != null)
                {
                    return playlist.Id;
                }

                offset += limit;
                hasMore = !string.IsNullOrEmpty(data.Next);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while getting playlist ID by name {PlaylistName} for user {UserId}", playlistName, spotifyUserId);
                break;
            }
        }

        return string.Empty;
    }

    public async Task<string> GetUserSpotifyIdByUserId(Guid userId)
    {
        var tool = await _uow.ToolsRepository.GetUserToolAsync(userId, ToolType.Spotify);

        if (tool == null) throw new EntityNotFoundException("User tool entity was not found");

        return tool.ToolAccountId;
    }

    public async Task<string> AddMusicToPlaylist(string playlistId, string accessToken, string songName, string? artistName = null)
    {
        using var client = _httpClientFactory.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var querySearch = $"{songName}";
            
        if (artistName != null) querySearch += $" artist:{artistName}";
        var query = new Dictionary<string, string>()
        {
            { "q", querySearch },
            { "type", "track" }
        };

        var responseUri = QueryHelpers.AddQueryString("https://api.spotify.com/v1/search", query);
        
        var response = await client.GetAsync(responseUri);

        var responseContent = await response.Content.ReadAsStringAsync(); 
        var encodedContent = Encoding.UTF8.GetBytes(responseContent);
        var jsonParse = JsonDocument.Parse(encodedContent);

        var items = jsonParse.RootElement.GetProperty("tracks").GetProperty("items");
        var firstItem = items[0];

        var songUri = firstItem.GetProperty("uri").GetString();
        var insertMusicResponse = await InsertMusicIntoPlaylist(playlistId, songUri, _accessToken);

        return insertMusicResponse;
    }

    private async Task<string> InsertMusicIntoPlaylist(string playlistId, string songUri, string _accessToken)
    {
        using var client = _httpClientFactory.CreateClient("https://api.spotify.com/v1/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_accessToken);

        try
        {
            var response = await client.PostAsJsonAsync($"playlists/{playlistId}/items", new
            {
                uris = new [] { songUri },
            });

            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.Created) return "Song added to playlist successfully";
        }
        catch (Exception e)
        {
            return "It was not possible to add the song to the playlist";
        }

        return "";
    }

}