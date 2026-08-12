using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.Services;

namespace TheAuctionHouse.Domain.Services.Tests;

public class AssetServiceBehaviorTests
{
    [Fact]
    public async Task UpdateAssetAsync_WithValidDraftAsset_UpdatesFields()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        var service = new AssetService(unitOfWork, new AuctionService(unitOfWork));

        await unitOfWork.AssetRepository.AddAsync(new Asset
        {
            UserId = 1,
            Title = "Original Asset",
            Description = "Original asset description.",
            RetailValue = 100,
            Status = AssetStatus.Draft
        });
        await unitOfWork.SaveChangesAsync();

        var asset = (await unitOfWork.AssetRepository.GetAllAsync()).Single();
        var result = await service.UpdateAssetAsync(new AssetInformationUpdateRequest
        {
            AssetId = asset.Id,
            Title = "Updated   Asset 01",
            Description = "Updated asset description.",
            RetailPrice = 250
        });

        Assert.True(result.IsSuccess);

        var updatedAsset = await unitOfWork.AssetRepository.GetByIdAsync(asset.Id);
        Assert.Equal("Updated Asset 01", updatedAsset!.Title);
        Assert.Equal(250, updatedAsset.RetailValue);
    }

    [Fact]
    public async Task DeleteAssetAsync_WithClosedAsset_ReturnsBadRequest()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        var service = new AssetService(unitOfWork, new AuctionService(unitOfWork));

        await unitOfWork.AssetRepository.AddAsync(new Asset
        {
            UserId = 1,
            Title = "Closed Asset",
            Description = "Closed asset description.",
            RetailValue = 100,
            Status = AssetStatus.ClosedForAuction
        });
        await unitOfWork.SaveChangesAsync();

        var asset = (await unitOfWork.AssetRepository.GetAllAsync()).Single();
        var result = await service.DeleteAssetAsync(asset.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.ErrorCode);
    }
}