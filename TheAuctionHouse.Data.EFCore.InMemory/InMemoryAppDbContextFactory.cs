using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheAuctionHouse.Data.EFCore.InMemory
{
    public class InMemoryAppDbContextFactory : IDesignTimeDbContextFactory<InMemoryAppDbContext>
    {
        public InMemoryAppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<InMemoryAppDbContext>();
            optionsBuilder.UseSqlite("Data Source=auctionhouse.db");
            return new InMemoryAppDbContext(optionsBuilder.Options);
        }
    }
}