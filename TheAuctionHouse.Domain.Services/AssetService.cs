using System.Text.RegularExpressions;
using System.Linq;
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

    public async Task<Result<bool>> CreateAssetAsync(AssetInformationUpdateRequest request, int userId)
    {
        var validationError = ValidateAssetRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        var asset = new Asset
        {
            UserId = userId,
            Title = NormalizeSpaces(request.Title),
            Description = request.Description.Trim(),
            RetailValue = request.RetailPrice,
            Status = AssetStatus.Draft
        };

        await _unitOfWork.AssetRepository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> DeleteAssetAsync(int assetId)
    {
        var asset = await _unitOfWork.AssetRepository.GetByIdAsync(assetId);
        if (asset is null)
        {
            return Error.NotFound("Asset not found.");
        }

        if (asset.Status is not AssetStatus.Draft and not AssetStatus.OpenToAuction)
        {
            return Error.BadRequest("Only assets in Draft or Open status can be deleted.");
        }

        await _unitOfWork.AssetRepository.DeleteAsync(asset);
        await _unitOfWork.AuctionRepository.DeleteAuctionsByAssetIdAsync(assetId);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Result<List<AssetResponse>>> GetAllAssetsByUserIdAsync(int userId)
    {
        var assets = await _unitOfWork.AssetRepository.GetAssetsByUserIdAsync(userId);
        var responses = new List<AssetResponse>();

        foreach (var asset in assets)
        {
            var owner = await _unitOfWork.PortalUserRepository.GetUserByUserIdAsync(asset.UserId);
            responses.Add(MapAssetResponse(asset, owner?.Name ?? string.Empty));
        }

        return responses;
    }

    public async Task<Result<AssetResponse>> GetAssetByIdAsync(int assetId)
    {
        var asset = await _unitOfWork.AssetRepository.GetByIdAsync(assetId);
        if (asset is null)
        {
            return Error.NotFound("Asset not found.");
        }

        var owner = await _unitOfWork.PortalUserRepository.GetUserByUserIdAsync(asset.UserId);
        return MapAssetResponse(asset, owner?.Name ?? string.Empty);
    }

    public async Task<PortalUserResponse> GetPortalUserByEmailAsync(string email)
    {
        var user = await _unitOfWork.PortalUserRepository.GetUserByEmailAsync(email);
        if (user is null)
        {
            return new PortalUserResponse();
        }

        return new PortalUserResponse
        {
            UserId = user.Id,
            Name = user.Name,
            EmailId = user.EmailId
        };
    }

    public async Task<Result<bool>> UpdateAssetAsync(AssetInformationUpdateRequest updateAssetRequest)
    {
        var validationError = ValidateAssetRequest(updateAssetRequest);
        if (validationError is not null)
        {
            return validationError;
        }

        var assets = await _unitOfWork.AssetRepository.GetAllAsync();
        var asset = assets.FirstOrDefault(existingAsset => existingAsset.Id == updateAssetRequest.AssetId);
        if (asset is null)
        {
            return Error.NotFound("Asset not found.");
        }

        if (asset.Status != AssetStatus.Draft)
        {
            return Error.BadRequest("Only assets in Draft status can be updated.");
        }

        asset.Title = NormalizeSpaces(updateAssetRequest.Title);
        asset.Description = updateAssetRequest.Description.Trim();
        asset.RetailValue = updateAssetRequest.RetailPrice;

        if (Enum.IsDefined(typeof(AssetStatus), updateAssetRequest.Status))
        {
            asset.Status = (AssetStatus)updateAssetRequest.Status;
        }

        await _unitOfWork.AssetRepository.UpdateAsync(asset);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static Error? ValidateAssetRequest(AssetInformationUpdateRequest request)
    {
        var normalizedTitle = NormalizeSpaces(request.Title);
        var description = request.Description.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTitle) || normalizedTitle.Length < 10 || normalizedTitle.Length > 150)
        {
            return Error.BadRequest("Title must be between 10 and 150 characters.");
        }

        if (!Regex.IsMatch(normalizedTitle, "^[A-Za-z0-9 ]+$"))
        {
            return Error.BadRequest("Title can contain only letters, numbers and spaces.");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length < 10 || description.Length > 1000)
        {
            return Error.BadRequest("Description must be between 10 and 1000 characters.");
        }

        if (request.RetailPrice <= 0)
        {
            return Error.BadRequest("Retail price must be a positive integer.");
        }

        return null;
    }

    private static string NormalizeSpaces(string value)
    {
        return Regex.Replace(value ?? string.Empty, "\\s+", " ").Trim();
    }

    private static AssetResponse MapAssetResponse(Asset asset, string ownerName)
    {
        return new AssetResponse
        {
            AssetId = asset.Id,
            Title = asset.Title,
            Description = asset.Description,
            RetailPrice = asset.RetailValue,
            OwnerName = ownerName,
            Status = asset.Status.ToString()
        };
    }
}