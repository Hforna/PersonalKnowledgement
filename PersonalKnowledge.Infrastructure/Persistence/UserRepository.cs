using Microsoft.EntityFrameworkCore;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Persistence;

public class UserRepository(DataContext context) : BaseRepository(context), IUserRepository
{
    public async Task<User?> GetUserByPhone(string phone)
    {
        return await _context.Users.FirstOrDefaultAsync(d => d.PhoneNumber == phone);
    }
}