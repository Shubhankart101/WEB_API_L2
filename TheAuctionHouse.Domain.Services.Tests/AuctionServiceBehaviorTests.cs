using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.Domain.Services.Tests;

public class AuctionServiceBehaviorTests
{
    [Fact]
    public async Task PlaceBidAsync_WithValidBid_BlocksBidderWalletAndStoresBidHistory()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        var service = new AuctionService(unitOfWork);

        await unitOfWork.PortalUserRepository.AddAsync(new PortalUser { Name = "Owner", EmailId = "owner@domain.com", HashedPassword = "pwd" });
        await unitOfWork.PortalUserRepository.AddAsync(new PortalUser { Name = "Bidder", EmailId = "bidder@domain.com", HashedPassword = "pwd" });
        await unitOfWork.SaveChangesAsync();

        var owner = await unitOfWork.PortalUserRepository.GetUserByEmailAsync("owner@domain.com");
        var bidder = await unitOfWork.PortalUserRepository.GetUserByEmailAsync("bidder@domain.com");

        await unitOfWork.AssetRepository.AddAsync(new Asset
        {
            UserId = owner!.Id,
            Title = "Auction Asset",
            Description = "Valid description for auction asset.",
            RetailValue = 500,
            Status = AssetStatus.OpenToAuction
        });
        await unitOfWork.SaveChangesAsync();

        var asset = (await unitOfWork.AssetRepository.GetAssetsByUserIdAsync(owner.Id)).Single();
        await unitOfWork.WalletRepository.AddAsync(new Wallet { UserId = bidder!.Id, Amount = 1000, BlockedAmount = 0 });
        await unitOfWork.AuctionRepository.AddAsync(new Auction
        {
            UserId = owner.Id,
            AssetId = asset.Id,
            ReservedPrice = 100,
            CurrentHighestBid = 0,
            CurrentHighestBidderId = 0,
            MinimumBidIncrement = 10,
            StartDate = DateTime.UtcNow,
            TotalMinutesToExpiry = 60,
            Status = AuctionStatus.Live
        });
        await unitOfWork.SaveChangesAsync();

        var auction = (await unitOfWork.AuctionRepository.GetAuctionsByUserIdAsync(owner.Id)).Single();
        var result = await service.PlaceBidAsync(new PlaceBidRequest
        {
            AuctionId = auction.Id,
            UserId = bidder.Id,
            BidAmount = 100
        });

        Assert.True(result.IsSuccess);

        var wallet = await unitOfWork.WalletRepository.GetByUserIdAsync(bidder.Id);
        var bids = await unitOfWork.AuctionRepository.GetBidHistoriesByAuctionIdAsync(auction.Id);

        Assert.Equal(100, wallet!.BlockedAmount);
        Assert.Single(bids);
    }

    [Fact]
    public async Task CheckAuctionExpiriesAsync_WithWinningBid_TransfersOwnershipAndSettlesWallets()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        var service = new AuctionService(unitOfWork);

        await unitOfWork.PortalUserRepository.AddAsync(new PortalUser { Name = "Seller", EmailId = "seller@domain.com", HashedPassword = "pwd" });
        await unitOfWork.PortalUserRepository.AddAsync(new PortalUser { Name = "Buyer", EmailId = "buyer@domain.com", HashedPassword = "pwd" });
        await unitOfWork.SaveChangesAsync();

        var seller = await unitOfWork.PortalUserRepository.GetUserByEmailAsync("seller@domain.com");
        var buyer = await unitOfWork.PortalUserRepository.GetUserByEmailAsync("buyer@domain.com");

        await unitOfWork.AssetRepository.AddAsync(new Asset
        {
            UserId = seller!.Id,
            Title = "Expired Asset",
            Description = "Valid description for expired asset.",
            RetailValue = 200,
            Status = AssetStatus.ClosedForAuction
        });
        await unitOfWork.WalletRepository.AddAsync(new Wallet { UserId = seller.Id, Amount = 0, BlockedAmount = 0 });
        await unitOfWork.WalletRepository.AddAsync(new Wallet { UserId = buyer!.Id, Amount = 500, BlockedAmount = 120 });
        await unitOfWork.SaveChangesAsync();

        var asset = (await unitOfWork.AssetRepository.GetAssetsByUserIdAsync(seller.Id)).Single();
        await unitOfWork.AuctionRepository.AddAsync(new Auction
        {
            UserId = seller.Id,
            AssetId = asset.Id,
            ReservedPrice = 100,
            CurrentHighestBid = 120,
            CurrentHighestBidderId = buyer.Id,
            MinimumBidIncrement = 10,
            StartDate = DateTime.UtcNow.AddMinutes(-90),
            TotalMinutesToExpiry = 60,
            Status = AuctionStatus.Live
        });
        await unitOfWork.SaveChangesAsync();

        var result = await service.CheckAuctionExpiriesAsync();

        Assert.True(result.IsSuccess);

        var updatedAsset = await unitOfWork.AssetRepository.GetByIdAsync(asset.Id);
        var sellerWallet = await unitOfWork.WalletRepository.GetByUserIdAsync(seller.Id);
        var buyerWallet = await unitOfWork.WalletRepository.GetByUserIdAsync(buyer.Id);

        Assert.Equal(buyer.Id, updatedAsset!.UserId);
        Assert.Equal(120, sellerWallet!.Amount);
        Assert.Equal(380, buyerWallet!.Amount);
        Assert.Equal(0, buyerWallet.BlockedAmount);
    }
}