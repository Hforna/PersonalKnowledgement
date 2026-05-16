using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Workers.Jobs;

public interface ISpotifyTokenRefreshJob
{
    Task RefreshAllSpotifyTokens();
}

public class SpotifyTokenRefreshJob(
    IUnitOfWork uow,
    ISpotifyAuthenticationService spotifyAuthenticationService,
    ILogger<SpotifyTokenRefreshJob> logger) : ISpotifyTokenRefreshJob
{
    public async Task RefreshAllSpotifyTokens()
    {
        logger.LogInformation("Starting Spotify token refresh job at {Time}", DateTime.UtcNow);

        var spotifyTools = await uow.ToolsRepository.GetAllToolsByTypeAsync(ToolType.Spotify);

        logger.LogInformation("Found {Count} Spotify tools to refresh", spotifyTools.Count);

        foreach (var tool in spotifyTools)
        {
            if (string.IsNullOrEmpty(tool.RefreshToken))
            {
                logger.LogWarning("Spotify tool for user {UserId} has no refresh token. Skipping.", tool.UserId);
                continue;
            }

            try
            {
                logger.LogInformation("Refreshing token for user {UserId}", tool.UserId);
                var tokens = await spotifyAuthenticationService.RefreshUserToken(tool.RefreshToken);

                if (tokens.RefreshToken is not null)
                    tool.RefreshToken = tokens.RefreshToken;

                tool.AccessToken = tokens.AccessToken;
                tool.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(tokens.ExpiresIn);

                uow.GenericRepository.Update(tool);
                logger.LogInformation("Successfully refreshed token for user {UserId}", tool.UserId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing Spotify token for user {UserId}", tool.UserId);
            }
        }

        await uow.CommitAsync();
        logger.LogInformation("Finished Spotify token refresh job at {Time}", DateTime.UtcNow);
    }
}
