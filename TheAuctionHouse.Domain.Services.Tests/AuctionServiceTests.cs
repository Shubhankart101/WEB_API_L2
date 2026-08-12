using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common; // or wherever Result<T> is defined

namespace TheAuctionHouse.Domain.Services.Tests;
public class AuctionServiceTests
{
    private readonly Mock<IAppUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuctionRepository> _auctionRepoMock;
    private readonly Mock<IAssetRepository> _assetRepoMock;
    private readonly AuctionService _service;

    public AuctionServiceTests()
    {
        _unitOfWorkMock = new Mock<IAppUnitOfWork>();
        _auctionRepoMock = new Mock<IAuctionRepository>();
        _assetRepoMock = new Mock<IAssetRepository>();

        _unitOfWorkMock.Setup(u => u.AuctionRepository).Returns(_auctionRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.AssetRepository).Returns(_assetRepoMock.Object);

        _service = new AuctionService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task PostAuctionAsync_WithInvalidReservedPrice_ReturnsValidationError()
    {
        var request = new PostAuctionRequest
        {
            AssetId = 1,
            OwnerId = 1,
            ReservedPrice = 0, // Invalid
            MinimumBidIncrement = 10,
            TotalMinutesToExpiry = 60
        };

        var result = await _service.PostAuctionAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.ErrorCode);
        Assert.Contains("Reserved price", result.Error.Message);
    }

    [Fact]
    public async Task PostAuctionAsync_AssetNotFound_ReturnsNotFound()
    {
        var request = new PostAuctionRequest
        {
            AssetId = 99,
            OwnerId = 1,
            ReservedPrice = 100,
            MinimumBidIncrement = 10,
            TotalMinutesToExpiry = 60
        };

        _assetRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Asset?)null);

        var result = await _service.PostAuctionAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error.ErrorCode);
        Assert.Contains("Asset not found", result.Error.Message);
    }

    [Fact]
    public async Task PostAuctionAsync_WithValidRequest_ReturnsSuccess()
    {
        var request = new PostAuctionRequest
        {
            AssetId = 1,
            OwnerId = 1,
            ReservedPrice = 100,
            MinimumBidIncrement = 10,
            TotalMinutesToExpiry = 60
        };

        _assetRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Asset { Id = 1 });
        _auctionRepoMock.Setup(r => r.AddAsync(It.IsAny<Auction>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result =await _service.PostAuctionAsync(request);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAuctionByIdAsync_AuctionNotFound_ReturnsNotFound()
    {
        _auctionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Auction?)null);

        var result = await _service.GetAuctionByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error.ErrorCode);
        Assert.Contains("Auction not found", result.Error.Message);
    }

    [Fact]
    public async Task GetAuctionByIdAsync_AuctionFound_ReturnsAuctionResponse()
    {
        var auction = new Auction
        {
            Id = 1,
            UserId = 2,
            AssetId = 3,
            ReservedPrice = 100,
            CurrentHighestBid = 0,
            CurrentHighestBidderId = 0,
            MinimumBidIncrement = 10,
            StartDate = DateTime.UtcNow,
            TotalMinutesToExpiry = 60,
            Status = AuctionStatus.Live
        };

        _auctionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(auction);
        // Mock asset lookup
        _assetRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Asset { Id = 3, Title = "Test Asset", Description = "Desc", RetailValue = 100, Status = AssetStatus.OpenToAuction });
        // Mock bid histories
        _auctionRepoMock.Setup(r => r.GetBidHistoriesByAuctionIdAsync(1)).ReturnsAsync(new List<BidHistory>());

        var result = await _service.GetAuctionByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(AuctionStatus.Live.ToString(), result.Value.Status);
    }
}