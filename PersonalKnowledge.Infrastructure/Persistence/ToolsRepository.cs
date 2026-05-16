using Microsoft.EntityFrameworkCore;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Persistence;

public class ToolsRepository(DataContext context) : BaseRepository(context), IToolsRepository
{
    public async Task<Tools?> GetUserToolAsync(Guid userId, ToolType type)
    {
        return await _context.Tools.FirstOrDefaultAsync(t => t.UserId == userId && t.Type == type);
    }

    public async Task<List<Tools>> GetAllToolsByTypeAsync(ToolType type)
    {
        return await _context.Tools.Where(d => d.Type == type).ToListAsync();
    }
}
