using Microsoft.EntityFrameworkCore;
using TheAuctionHouse.Data.EFCore.InMemory;

namespace TheAuctionHouse.Domain.Services.Tests;

internal static class TestAppUnitOfWorkFactory
{
    public static InMemoryAppUnitOfWork Create()
    {
        var options = new DbContextOptionsBuilder<InMemoryAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InMemoryAppUnitOfWork(new InMemoryAppDbContext(options));
    }
}