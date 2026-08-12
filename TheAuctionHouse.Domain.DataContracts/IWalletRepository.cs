using System.Threading.Tasks;
using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.Domain.DataContracts
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(int userId);
        Task AddAsync(Wallet wallet);
        Task UpdateAsync(Wallet wallet);
    }
}