using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Services;
using PersonalKnowledge.Results;

namespace PersonalKnowledge.Controllers;

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
        await _documentService.UploadFile(files);
        return Ok(ApiResult.Success("Documents uploaded and queued for processing successfully"));
    }
}