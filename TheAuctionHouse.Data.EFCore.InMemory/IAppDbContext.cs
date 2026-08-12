using TheAuctionHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<T>? GetDbSet<T>() where T : class;
    IQueryable<PortalUser> PortalUsers { get; }
    IQueryable<Asset> Assets { get; }
    IQueryable<Auction> Auctions { get; }
    IQueryable<BidHistory> BidHistories { get; }
    IQueryable<Wallet> Wallets { get; }
}