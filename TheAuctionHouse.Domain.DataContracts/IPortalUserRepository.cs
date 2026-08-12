namespace TheAuctionHouse.Domain.DataContracts;

using System.Collections.Generic;
using TheAuctionHouse.Domain.Entities;

public interface IPortalUserRepository : IRepository<PortalUser>
{
    Task<PortalUser?> GetUserByUserIdAsync(int userId);

    Task<PortalUser?> GetUserByEmailAsync(string email);

    void DepositWalletBalance(int userId, int amount);
    void WithdrawWalletBalance(int userId, int amount);
}