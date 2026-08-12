using SKUApp.Data.EFCore.InMemory;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using System.Linq; // Add this at the top if not present
using Microsoft.EntityFrameworkCore; // Add this at the top

public class InMemoryAuctionRepository : GenericRepository<Auction>, IAuctionRepository
{
    public InMemoryAuctionRepository(IAppDbContext context) : base(context)
    {
    }

    public async Task<List<Auction>> GetAuctionsByUserIdAsync(int userId)
    {
        return await _context.Auctions
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<BidHistory>> GetBidHistoriesByAuctionIdAsync(int auctionId)
    {
        return await _context.BidHistories
            .Where(b => b.AuctionId == auctionId)
            .ToListAsync();
    }

    public async Task<List<BidHistory>> GetBidHistoriesByUserIdAsync(int userId)
    {
        return await _context.BidHistories
            .Where(b => b.BidderId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(BidHistory bidHistory)
    {
        await _context.BidHistories.AddAsync(bidHistory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAuctionsByAssetIdAsync(int assetId)
    {
        var auctions = _context.Auctions.Where(a => a.AssetId == assetId).ToList();
        if (auctions.Any())
        {
            _context.Auctions.RemoveRange(auctions);
            await _context.SaveChangesAsync();
        }
    }
}