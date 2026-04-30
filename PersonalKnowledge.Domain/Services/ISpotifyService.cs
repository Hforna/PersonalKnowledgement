namespace PersonalKnowledge.Domain.Services;

public interface ISpotifyService
{
    public Task<string> GetPlaylistIdByName(string playlistName, string spotifyUserId, string accessToken);
    public Task<string> GetUserSpotifyIdByUserId(Guid userId);
    public Task<string> AddMusicToPlaylist(string playlistId, string accessToken, string songName, string? artistName = null);
}