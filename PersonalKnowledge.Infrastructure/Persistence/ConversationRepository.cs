using Microsoft.EntityFrameworkCore;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;
using X.PagedList;

namespace PersonalKnowledge.Infrastructure.Persistence;

public class ConversationRepository(DataContext context) : BaseRepository(context), IConversationRepository
{
    public async Task<IPagedList<Conversation>> GetConversationsPaginatedAsync(int page, int pageSize)
    {
        var total = await _context.Conversations.CountAsync();
        var items = await _context.Conversations
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return new StaticPagedList<Conversation>(items, page, pageSize, total);
    }

    public async Task DeleteConversationAsync(Guid id)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sources)
            .SingleOrDefaultAsync(c => c.Id == id);

        if (conversation != null)
        {
            _context.Conversations.Remove(conversation);
        }
    }

    public async Task<List<Conversation>?> GetUserConversationBySource(Guid userId, ConversationSource source)
    {
        return await _context.Conversations.Where(d => d.UserId == userId && d.ConversationSource == source).ToListAsync();
    }
}
