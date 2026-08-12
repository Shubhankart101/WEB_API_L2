using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class EfCoreWalletRepository : IWalletRepository
    {
        private readonly InMemoryAppDbContext _context;
        public EfCoreWalletRepository(InMemoryAppDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByUserIdAsync(int userId)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _context.Wallets.AddAsync(wallet);
        }

        public Task UpdateAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            return Task.CompletedTask;
        }
    }
}
