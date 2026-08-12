using System.Text.RegularExpressions;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;

namespace TheAuctionHouse.Domain.Services;

public class AssetService : IAssetService
{
    private readonly IAppUnitOfWork _unitOfWork;
    private readonly IAuctionService _auctionService;

    public AssetService(IAppUnitOfWork unitOfWork, IAuctionService auctionService)
    {
        _unitOfWork = unitOfWork;
        _auctionService = auctionService;
    }

    public Task<Result<bool>> CreateAssetAsync(AssetInformationUpdateRequest request, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> DeleteAssetAsync(int assetId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<AssetResponse>>> GetAllAssetsByUserIdAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AssetResponse>> GetAssetByIdAsync(int assetId)
    {
        throw new NotImplementedException();
    }

    public Task<PortalUserResponse> GetPortalUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> UpdateAssetAsync(AssetInformationUpdateRequest updateAssetRequest)
    {
        throw new NotImplementedException();
    }
}