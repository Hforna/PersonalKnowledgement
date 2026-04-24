namespace PersonalKnowledge.Domain.Services;

public interface IUserRepository
{
    public Task<User?> GetUserByPhone(string phone);
}