using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.DataContracts;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class InMemoryAppUnitOfWork : IAppUnitOfWork
    {
        public IWalletRepository WalletRepository { get; }
        private readonly InMemoryAppDbContext _context;
        private bool _disposed;

        public InMemoryAppUnitOfWork(InMemoryAppDbContext context)
        {
            _context = context;
            PortalUserRepository = new InMemoryPortalUserRespository(_context);
            AssetRepository = new InMemoryAssetRepository(_context);
            AuctionRepository = new InMemoryAuctionRepository(_context);
            WalletRepository = new EfCoreWalletRepository(_context); // Use EF Core-based repo
        }

        public IPortalUserRepository PortalUserRepository { get; private set; }

        public IAssetRepository AssetRepository { get; private set; }

        public IAuctionRepository AuctionRepository { get; private set; }

        public async Task<int> SaveChangesAsync()
        {
            // In-memory implementation, so just return 0 changes
            //return await Task.FromResult(0);
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    _context.Dispose();
                }

                // Dispose unmanaged resources here
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}