using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;

namespace PersonalKnowledge.Domain.Services;

public interface IToolsRepository
{
    public Task<Tools?> GetUserToolAsync(Guid userId, ToolType type);
}
