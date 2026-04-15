using Microsoft.EntityFrameworkCore;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Persistence;

public class GenericRepository(DataContext context) : BaseRepository(context), IGenericRepository
{
    public async Task<T?> GetByIdAsync<T>(Guid id) where T : Entity
    {
        return await _context.Set<T>().SingleOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddAsync<T>(T entity) where T : Entity
    {
        await _context.Set<T>().AddAsync(entity);       
    }

    public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : Entity
    {
        await _context.Set<T>().AddRangeAsync(entities);
    }

    public void Update<T>(T entity) where T : Entity
    {
        _context.Set<T>().Update(entity);
    }
}