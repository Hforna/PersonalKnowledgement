using Microsoft.EntityFrameworkCore;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Helpers;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Persistence;

public class UserRepository(DataContext context) : BaseRepository(context), IUserRepository
{
    public async Task<User?> GetUserByPhone(string phone)
    {
        var normalizedPhone = PhoneHelper.NormalizePhoneNumber(phone);
        return await _context.Users.FirstOrDefaultAsync(d => d.PhoneNumber == normalizedPhone);
    }

    public async Task<User?> UserById(Guid userId)
    {
        return await _context.Users.SingleOrDefaultAsync(d => d.Id == userId && d.IsActive);
    }
}