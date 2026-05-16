namespace PersonalKnowledge.Domain.Services;

public interface IUserRepository
{
    public Task<User?> GetUserByPhone(string phone);
    public Task<User?> UserById(Guid userId);
}