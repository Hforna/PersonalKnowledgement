using PersonalKnowledge.Domain.Entities;
using X.PagedList;

namespace PersonalKnowledge.Domain.Services;

public interface IConversationRepository
{
    Task<IPagedList<Conversation>> GetConversationsPaginatedAsync(int page, int pageSize);
    Task DeleteConversationAsync(Guid id);
}
