namespace TheAuctionHouse.Domain.DataContracts;

using System.Collections.Generic;
using TheAuctionHouse.Domain.Entities;

public interface IAuctionRepository : IRepository<Auction>
{
    Task<List<Auction>> GetAuctionsByUserIdAsync(int userId);

    Task<List<BidHistory>> GetBidHistoriesByUserIdAsync(int userId);
    Task<List<BidHistory>> GetBidHistoriesByAuctionIdAsync(int userId);
    Task AddAsync(BidHistory bidHistory);
    Task DeleteAuctionsByAssetIdAsync(int assetId);
}