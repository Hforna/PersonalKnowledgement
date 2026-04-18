using Microsoft.AspNetCore.Http;

namespace PersonalKnowledge.Application.Requests;

public class UploadAssetRequest
{
    public IFormFile File { get; set; }
    public string Label { get; set; }
}