namespace PersonalKnowledge.Infrastructure.Persistence;

public abstract class BaseRepository(DataContext context)
{
    protected readonly DataContext _context = context;
}