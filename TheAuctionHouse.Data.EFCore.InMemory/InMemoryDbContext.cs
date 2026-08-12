namespace TheAuctionHouse.Data.EFCore.InMemory;

using Microsoft.EntityFrameworkCore;

using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.DataContracts;
using System.Linq;

public class InMemoryAppDbContext : DbContext, IAppDbContext
    {
    public InMemoryAppDbContext(DbContextOptions<InMemoryAppDbContext> options)
        : base(options)
    {
        PortalUsers = Set<PortalUser>();
        Assets = Set<Asset>();
        Auctions = Set<Auction>();
        BidHistories = Set<BidHistory>();
        Wallets = Set<Wallet>(); // Add this line
        }

        public DbSet<T>? GetDbSet<T>() where T : class
        {
            if (typeof(T) == typeof(PortalUser))
                return PortalUsers as DbSet<T>;
            if (typeof(T) == typeof(Asset))
                return Assets as DbSet<T>;
            if (typeof(T) == typeof(Auction))
                return Auctions as DbSet<T>;
            if (typeof(T) == typeof(BidHistory))
                return BidHistories as DbSet<T>;
                if (typeof(T) == typeof(Wallet))
            return Wallets as DbSet<T>; // Add this line

            throw new ArgumentException("Invalid type");
        }

        public DbSet<PortalUser> PortalUsers { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<BidHistory> BidHistories { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
    IQueryable<PortalUser> IAppDbContext.PortalUsers => PortalUsers;

    IQueryable<Asset> IAppDbContext.Assets => Assets;

    IQueryable<Auction> IAppDbContext.Auctions => Auctions;

    IQueryable<BidHistory> IAppDbContext.BidHistories => BidHistories;

     IQueryable<Wallet> IAppDbContext.Wallets => Wallets; // Add this line
   
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        // Force SQLite for permanent storage
        optionsBuilder.UseSqlite("Data Source=auctionhouse.db");
    }
}

        // Define your DbSets here
    // public DbSet<YourEntity> YourEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure your entity relationships and constraints here
    }

    }