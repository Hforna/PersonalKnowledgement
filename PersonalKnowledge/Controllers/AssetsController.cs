using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Requests;
using PersonalKnowledge.Application.Services;
using PersonalKnowledge.Results;

namespace PersonalKnowledge.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    
    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }
    
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResult>> UploadAsset([FromForm]UploadAssetRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        await _assetService.UploadAsset(request);
        return Ok(ApiResult.Success("Asset uploaded and queued for processing successfully"));
    }
}