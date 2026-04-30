namespace PersonalKnowledge.Domain.Services
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenericRepository GenericRepository { get; }
        public IAssetRepository AssetRepository { get; }
        public IConversationRepository ConversationRepository { get; }
        public IUserRepository UserRepository { get; }
        public IToolsRepository ToolsRepository { get; }
        Task<int> SaveChangesAsync();
        Task<bool> BeginTransactionAsync();
        Task<bool> CommitAsync();
        Task<bool> RollbackAsync();
    }
}
