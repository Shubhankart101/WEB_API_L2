using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.DataContracts;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class InMemoryWalletRepository : IWalletRepository
    {
        private readonly List<Wallet> _wallets = new();

        public Task<Wallet?> GetByUserIdAsync(int userId)
        {
            var wallet = _wallets.FirstOrDefault(w => w.UserId == userId);
            return Task.FromResult(wallet);
        }

        public Task AddAsync(Wallet wallet)
        {
            _wallets.Add(wallet);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Wallet wallet)
        {
            // In-memory: nothing needed, as the object is already updated by reference
            return Task.CompletedTask;
        }
    }
}