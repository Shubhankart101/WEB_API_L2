using System.Collections.Generic;
using System.Threading.Tasks;
using TheAuctionHouse.Common.ErrorHandling;
public interface IAssetService
{
    Task<Result<bool>> CreateAssetAsync(AssetInformationUpdateRequest request, int userId);
    Task<Result<bool>> UpdateAssetAsync(AssetInformationUpdateRequest updateAssetRequest);
    Task<Result<bool>> DeleteAssetAsync(int assetId);
    Task<Result<AssetResponse>> GetAssetByIdAsync(int assetId);
    Task<Result<List<AssetResponse>>> GetAllAssetsByUserIdAsync(int userId);
    Task<PortalUserResponse> GetPortalUserByEmailAsync(string email);
}