using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Application.Dtos;
using X.PagedList;

namespace PersonalKnowledge.Application.Services;

public interface IConversationService
{
    Task<Guid> CreateConversation(string title);
    Task<IPagedList<ConversationDto>> GetConversationsPaginated(int page, int pageSize);
    Task<Conversation?> GetConversationById(Guid id);
    Task DeleteConversation(Guid id);
}

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _uow;

    public ConversationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> CreateConversation(string title)
    {
        var conversation = new Conversation
        {
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.GenericRepository.AddAsync(conversation);
        await _uow.CommitAsync();

        return conversation.Id;
    }

    public async Task<IPagedList<ConversationDto>> GetConversationsPaginated(int page, int pageSize)
    {
        var pagedResult = await _uow.ConversationRepository.GetConversationsPaginatedAsync(page, pageSize);
        
        var dtos = pagedResult.Select(c => new ConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });

        return new StaticPagedList<ConversationDto>(dtos, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalItemCount);
    }

    public async Task<Conversation?> GetConversationById(Guid id)
    {
        return await _uow.GenericRepository.GetByIdAsync<Conversation>(id);
    }

    public async Task DeleteConversation(Guid id)
    {
        await _uow.ConversationRepository.DeleteConversationAsync(id);
        await _uow.CommitAsync();
    }
}