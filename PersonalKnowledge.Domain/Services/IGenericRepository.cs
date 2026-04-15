namespace PersonalKnowledge.Domain.Services;

public interface IGenericRepository
{
    public Task<T?> GetByIdAsync<T>(Guid id) where T : Entity;
    public Task AddAsync<T>(T entity) where T : Entity;   
    public Task AddRangeAsync<T>(IEnumerable<T> entities) where T : Entity;  
    public void Update<T>(T entity) where T: Entity;
}