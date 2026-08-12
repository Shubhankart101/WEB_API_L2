namespace TheAuctionHouse.Domain.DataContracts;

using System.Collections.Generic;
using TheAuctionHouse.Domain.Entities;

public interface IAssetRepository : IRepository<Asset>
{
    Task<List<Asset>> GetAssetsByUserIdAsync(int userId);
    
}