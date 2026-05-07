using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Services;

namespace PersonalKnowledge.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class ToolsController : ControllerBase
{
    private readonly IToolsService _toolsService;

    public ToolsController(IToolsService toolsService)
    {
        _toolsService = toolsService;
    }

    [HttpPost("spotify/connect/{phoneNumber}")]
    public async Task<IActionResult> ConnectSpotifyAccount([FromRoute]string phoneNumber)
    {
        var uri = await _toolsService.ConnectSpotifyRequest(phoneNumber);
        
        return Redirect(uri);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        await _toolsService.UpdateSpotifyRefreshToken(Guid.NewGuid());

        return Ok();
    }

    [HttpGet("spotify/callback")]
    public async Task<IActionResult> SpotifyCallback([FromQuery]string code, [FromQuery]string state, [FromQuery]string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            return BadRequest();

        await _toolsService.HandleSpotifyAuthenticationCallback(state, code, "https://d77e-2804-d51-4451-4600-f4b3-35a8-7ff0-6a54.ngrok-free.app/api/tools/spotify/callback");
        
        return Ok();
    }
}