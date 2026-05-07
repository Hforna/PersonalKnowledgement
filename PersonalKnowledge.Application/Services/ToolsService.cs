using PersonalKnowledge.Domain.Dtos;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Exceptions;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public interface IToolsService
{
    public Task<string> ConnectSpotifyRequest(string phoneNumber);
    public Task UpdateSpotifyRefreshToken(Guid toolId);
    public Task HandleSpotifyAuthenticationCallback(string state, string code, string redirectUri);
}

public class ToolsService : IToolsService
{
    private readonly ISpotifyAuthenticationService _spotifyAuthenticationService;
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly ITokenService _tokenService;
    private readonly ISpotifyService _spotifyService;

    public ToolsService(ISpotifyAuthenticationService spotifyAuthenticationService, IUnitOfWork uow, IMemoryCacheService memoryCacheService, ITokenService tokenService, ISpotifyService spotifyService)
    {
        _spotifyAuthenticationService = spotifyAuthenticationService;
        _uow = uow;
        _spotifyService = spotifyService;
        _memoryCacheService = memoryCacheService;
        _tokenService = tokenService;
    }


    public async Task<string> ConnectSpotifyRequest(string phoneNumber)
    {
        var user = await _tokenService.GetUserByTokenAsync();

        if (user is null)
            user = await _uow.UserRepository.GetUserByPhone(phoneNumber) ?? throw new EntityNotFoundException("The user by phone number was not found");
        
        var state = Guid.NewGuid().ToString();

        var isSet = _memoryCacheService.SetValue(state, user.Id.ToString());

        if (!isSet)
            throw new StorageException("It was not possible to save the state value");
        
        return _spotifyAuthenticationService.BuildAuthenticationUri(state);
    }

    public async Task UpdateSpotifyRefreshToken(Guid userId)
    {
        var user = await _tokenService.GetUserByTokenAsync();
        
        var tool = await _uow.ToolsRepository.GetUserToolAsync(user.Id, ToolType.Spotify)
            ?? throw new EntityNotFoundException("User's Spotify tool not found");

        var tokens = await _spotifyAuthenticationService.RefreshUserToken(tool.RefreshToken);

        if(tokens.RefreshToken is not null)
            tool.RefreshToken = tokens.RefreshToken;
        
        tool.AccessToken = tokens.AccessToken;
        tool.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(tokens.ExpiresIn);
        
        _uow.GenericRepository.Update(tool);
        await _uow.CommitAsync();
    }

    public async Task HandleSpotifyAuthenticationCallback(string state, string code, string redirectUri)
    {
        var tokens = await _spotifyAuthenticationService.GetAccessToken(code, redirectUri, state);

        var userId = Guid.Parse(_memoryCacheService.GetValueByKey(state));

        var spotifyUserId = await _spotifyService.GetUserSpotifyId(tokens.AccessToken);

        var tool = new Tools()
        {
            Type = ToolType.Spotify,
            UserId = userId,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ToolAccountId = spotifyUserId,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(tokens.ExpiresIn)
        };

        await _uow.GenericRepository.AddAsync(tool);
        await _uow.CommitAsync();
    }
}