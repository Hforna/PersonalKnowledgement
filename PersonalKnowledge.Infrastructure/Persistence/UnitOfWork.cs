using Microsoft.EntityFrameworkCore.Storage;
using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Infrastructure;

namespace PersonalKnowledge.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(DataContext context, IGenericRepository genericRepository, IAssetRepository assetRepository, IConversationRepository conversationRepository)
        {
            _context = context;
            GenericRepository = genericRepository;
            AssetRepository = assetRepository;
            ConversationRepository = conversationRepository;
        }

        public IGenericRepository GenericRepository { get; }
        public IAssetRepository AssetRepository { get; }
        public IConversationRepository ConversationRepository { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return true;
        }

        public async Task<bool> CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
                return true;
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task<bool> RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            catch
            {
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            return true;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
