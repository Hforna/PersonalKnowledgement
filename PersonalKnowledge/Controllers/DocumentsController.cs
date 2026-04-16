using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Services;
using PersonalKnowledge.Results;

namespace PersonalKnowledge.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    
    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }
    
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResult>> UploadDocument([FromForm]List<IFormFile> files)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        await _documentService.UploadFile(files, userId);
        return Ok(ApiResult.Success("Documents uploaded and queued for processing successfully"));
    }
}