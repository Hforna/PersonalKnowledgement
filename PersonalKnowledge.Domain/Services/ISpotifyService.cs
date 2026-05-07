namespace PersonalKnowledge.Domain.Services;

public interface ISpotifyService
{
    public string AuthenticationUrl { get; set; }
    public Task<string> GetPlaylistIdByName(string playlistName, string spotifyUserId, string accessToken);
    public Task<string> GetUserSpotifyId(string accessToken);
    public Task<string> AddMusicToPlaylist(string playlistId, string accessToken, string songName, string? artistName = null);
}