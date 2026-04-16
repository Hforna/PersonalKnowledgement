namespace PersonalKnowledge.Domain.Services
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenericRepository GenericRepository { get; }
        public IDocumentRepository DocumentRepository { get; }
        public IConversationRepository ConversationRepository { get; }
        Task<int> SaveChangesAsync();
        Task<bool> BeginTransactionAsync();
        Task<bool> CommitAsync();
        Task<bool> RollbackAsync();
    }
}
