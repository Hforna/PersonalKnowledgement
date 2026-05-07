using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Requests;
using PersonalKnowledge.Application.Services;
using PersonalKnowledge.Results;

namespace PersonalKnowledge.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;

    public ConversationsController(IConversationService conversationService, IMessageService messageService)
    {
        _conversationService = conversationService;
        _messageService = messageService;       
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendMessageToConversation(Guid id, [FromBody] SendMessageRequest request)
    {
        var result = await _messageService.SendMessage(request);
        
        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResult<Guid>>> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var id = await _conversationService.CreateConversation(request.Title);
        return Ok(ApiResult<Guid>.Success(id));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult>> GetConversationById(Guid id)
    {
        var conversation = await _conversationService.GetConversationById(id);
        if (conversation == null)
            return NotFound(ApiResult.Failure("Conversation not found", 404));

        return Ok(ApiResult<object>.Success(conversation));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult>> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _conversationService.GetConversationsPaginated(page, pageSize);

        return Ok(ApiResult<object>.Success(new
        {
            Items = result,
            Total = result.TotalItemCount,
            Page = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.PageCount
        }));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResult>> DeleteConversation(Guid id)
    {
        await _conversationService.DeleteConversation(id);
        return Ok(ApiResult.Success("Conversation deleted successfully"));
    }
}

public class CreateConversationRequest
{
    public string Title { get; set; } = "New Conversation";
}