using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;

public class AuctionService : IAuctionService
{
    private readonly IAppUnitOfWork _unitOfWork;

    public AuctionService(IAppUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> CheckAuctionExpiriesAsync()
    {
        var auctions = await _unitOfWork.AuctionRepository.GetAllAsync();

        foreach (var auction in auctions.Where(currentAuction => currentAuction.Status == AuctionStatus.Live && currentAuction.IsExpired()))
        {
            var asset = await _unitOfWork.AssetRepository.GetByIdAsync(auction.AssetId);
            if (asset is null)
            {
                continue;
            }

            if (auction.CurrentHighestBid <= 0 || auction.CurrentHighestBidderId <= 0)
            {
                auction.Status = AuctionStatus.ExpiredWithoutBids;
                asset.Status = AssetStatus.OpenToAuction;
            }
            else
            {
                auction.Status = AuctionStatus.Expired;
                asset.UserId = auction.CurrentHighestBidderId;
                asset.Status = AssetStatus.OpenToAuction;
                await SettleAuctionWalletsAsync(auction);
            }

            await _unitOfWork.AssetRepository.UpdateAsync(asset);
            await _unitOfWork.AuctionRepository.UpdateAsync(auction);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Result<List<AuctionResponse>>> GetAllOpenAuctionsByUserIdAsync()
    {
        var auctions = await _unitOfWork.AuctionRepository.GetAllAsync();
        var liveAuctions = auctions
            .Where(auction => auction.Status == AuctionStatus.Live && !auction.IsExpired())
            .OrderBy(auction => auction.GetRemainingTimeInMinutes())
            .ToList();

        return await MapAuctionsAsync(liveAuctions);
    }

    public async Task<Result<AuctionResponse>> GetAuctionByIdAsync(int auctionId)
    {
        var auction = await _unitOfWork.AuctionRepository.GetByIdAsync(auctionId);
        if (auction is null)
        {
            return Error.NotFound("Auction not found.");
        }

        return await MapAuctionAsync(auction);
    }

    public async Task<Result<List<AuctionResponse>>> GetAuctionsByUserIdAsync(int userId)
    {
        var auctions = await _unitOfWork.AuctionRepository.GetAuctionsByUserIdAsync(userId);
        return await MapAuctionsAsync(auctions);
    }

    public async Task<Result<bool>> PlaceBidAsync(PlaceBidRequest placeBidRequest)
    {
        if (placeBidRequest.AuctionId <= 0 || placeBidRequest.UserId <= 0)
        {
            return Error.BadRequest("AuctionId and UserId must be greater than zero.");
        }

        var auction = await _unitOfWork.AuctionRepository.GetByIdAsync(placeBidRequest.AuctionId);
        if (auction is null)
        {
            return Error.NotFound("Auction not found.");
        }

        if (auction.IsExpired() || auction.Status != AuctionStatus.Live)
        {
            return Error.BadRequest("Auction is not open for bidding.");
        }

        if (auction.UserId == placeBidRequest.UserId)
        {
            return Error.BadRequest("Owner cannot place a bid on their own auction.");
        }

        var minimumBid = auction.CurrentHighestBid > 0
            ? auction.CurrentHighestBid + auction.MinimumBidIncrement
            : auction.ReservedPrice;

        if (placeBidRequest.BidAmount < minimumBid)
        {
            return Error.BadRequest($"Bid amount must be at least {minimumBid}.");
        }

        var bidderWallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(placeBidRequest.UserId);
        if (bidderWallet is null)
        {
            return Error.NotFound("Wallet not found.");
        }

        var availableBalance = bidderWallet.Amount - bidderWallet.BlockedAmount;
        if (availableBalance < placeBidRequest.BidAmount)
        {
            return Error.BadRequest("Insufficient available balance.");
        }

        if (auction.CurrentHighestBidderId > 0)
        {
            var previousHighestBidderWallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(auction.CurrentHighestBidderId);
            if (previousHighestBidderWallet is not null)
            {
                previousHighestBidderWallet.BlockedAmount = Math.Max(0, previousHighestBidderWallet.BlockedAmount - auction.CurrentHighestBid);
                await _unitOfWork.WalletRepository.UpdateAsync(previousHighestBidderWallet);
            }
        }

        bidderWallet.BlockedAmount += placeBidRequest.BidAmount;
        await _unitOfWork.WalletRepository.UpdateAsync(bidderWallet);

        var bidder = await _unitOfWork.PortalUserRepository.GetUserByUserIdAsync(placeBidRequest.UserId);
        auction.CurrentHighestBid = placeBidRequest.BidAmount;
        auction.CurrentHighestBidderId = placeBidRequest.UserId;
        await _unitOfWork.AuctionRepository.UpdateAsync(auction);

        await _unitOfWork.AuctionRepository.AddAsync(new BidHistory
        {
            AuctionId = auction.Id,
            BidderId = placeBidRequest.UserId,
            BidderName = bidder?.Name ?? string.Empty,
            BidAmount = placeBidRequest.BidAmount,
            BidDate = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Result<bool>> PostAuctionAsync(PostAuctionRequest postAuctionRequest)
    {
        var validationError = ValidatePostAuctionRequest(postAuctionRequest);
        if (validationError is not null)
        {
            return validationError;
        }

        var asset = await _unitOfWork.AssetRepository.GetByIdAsync(postAuctionRequest.AssetId);
        if (asset is null)
        {
            return Error.NotFound("Asset not found.");
        }

        if (asset.UserId != postAuctionRequest.OwnerId)
        {
            return Error.BadRequest("Asset does not belong to the specified owner.");
        }

        if (asset.Status != AssetStatus.OpenToAuction)
        {
            return Error.BadRequest("Only assets in Open status can be posted for auction.");
        }

        var auction = new Auction
        {
            UserId = postAuctionRequest.OwnerId,
            AssetId = postAuctionRequest.AssetId,
            ReservedPrice = postAuctionRequest.ReservedPrice,
            CurrentHighestBid = 0,
            CurrentHighestBidderId = 0,
            MinimumBidIncrement = postAuctionRequest.MinimumBidIncrement,
            StartDate = DateTime.UtcNow,
            TotalMinutesToExpiry = postAuctionRequest.TotalMinutesToExpiry,
            Status = AuctionStatus.Live
        };

        asset.Status = AssetStatus.ClosedForAuction;

        await _unitOfWork.AuctionRepository.AddAsync(auction);
        await _unitOfWork.AssetRepository.UpdateAsync(asset);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static Error? ValidatePostAuctionRequest(PostAuctionRequest request)
    {
        if (request.AssetId <= 0 || request.OwnerId <= 0)
        {
            return Error.BadRequest("AssetId and OwnerId must be greater than zero.");
        }

        if (request.ReservedPrice <= 0 || request.ReservedPrice > 9999)
        {
            return Error.BadRequest("Reserved price must be between 1 and 9999.");
        }

        if (request.MinimumBidIncrement <= 0 || request.MinimumBidIncrement > 999)
        {
            return Error.BadRequest("Minimum bid increment must be between 1 and 999.");
        }

        if (request.TotalMinutesToExpiry <= 0 || request.TotalMinutesToExpiry > 10080)
        {
            return Error.BadRequest("Expiration time must be between 1 and 10080 minutes.");
        }

        return null;
    }

    private async Task<Result<List<AuctionResponse>>> MapAuctionsAsync(IEnumerable<Auction> auctions)
    {
        var responses = new List<AuctionResponse>();
        foreach (var auction in auctions)
        {
            var responseResult = await MapAuctionAsync(auction);
            if (!responseResult.IsSuccess || responseResult.Value is null)
            {
                return responseResult.Error;
            }

            responses.Add(responseResult.Value);
        }

        return responses;
    }

    private async Task<Result<AuctionResponse>> MapAuctionAsync(Auction auction)
    {
        var asset = await _unitOfWork.AssetRepository.GetByIdAsync(auction.AssetId);
        if (asset is null)
        {
            return Error.NotFound("Asset not found for auction.");
        }

        var highestBidder = auction.CurrentHighestBidderId > 0
            ? await _unitOfWork.PortalUserRepository.GetUserByUserIdAsync(auction.CurrentHighestBidderId)
            : null;
        var bidHistories = await _unitOfWork.AuctionRepository.GetBidHistoriesByAuctionIdAsync(auction.Id);

        return new AuctionResponse
        {
            AuctionId = auction.Id,
            UserId = auction.UserId,
            AssetId = auction.AssetId,
            AssetTitle = asset.Title,
            AssetDescription = asset.Description,
            CurrentHighestBid = auction.CurrentHighestBid,
            CurrentHighestBidderId = auction.CurrentHighestBidderId,
            HighestBidderName = highestBidder?.Name ?? string.Empty,
            MinimumBidIncrement = auction.MinimumBidIncrement,
            ReservedPrice = auction.ReservedPrice,
            StartDate = auction.StartDate,
            TotalMinutesToExpiry = auction.TotalMinutesToExpiry,
            Status = auction.Status.ToString(),
            CustomStatusMessage = GetStatusMessage(auction),
            BidHistory = bidHistories
                .OrderByDescending(bid => bid.BidDate)
                .Select(bid => new BidHistoryResponse
                {
                    BidId = bid.Id,
                    AuctionId = bid.AuctionId,
                    UserId = bid.BidderId,
                    BidAmount = bid.BidAmount,
                    BidTime = bid.BidDate,
                    UserName = bid.BidderName
                })
                .ToList()
        };
    }

    private static string GetStatusMessage(Auction auction)
    {
        if (auction.IsExpiredWithoutBids())
        {
            return "Auction expired without bids.";
        }

        if (auction.IsExpired())
        {
            return "Auction expired.";
        }

        return $"Auction closes in {Math.Max(0, auction.GetRemainingTimeInMinutes())} minutes.";
    }

    private async Task SettleAuctionWalletsAsync(Auction auction)
    {
        var buyerWallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(auction.CurrentHighestBidderId);
        if (buyerWallet is not null)
        {
            buyerWallet.BlockedAmount = Math.Max(0, buyerWallet.BlockedAmount - auction.CurrentHighestBid);
            buyerWallet.Amount -= auction.CurrentHighestBid;
            await _unitOfWork.WalletRepository.UpdateAsync(buyerWallet);
        }

        var sellerWallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(auction.UserId);
        if (sellerWallet is null)
        {
            sellerWallet = new Wallet
            {
                UserId = auction.UserId,
                Amount = auction.CurrentHighestBid,
                BlockedAmount = 0
            };
            await _unitOfWork.WalletRepository.AddAsync(sellerWallet);
        }
        else
        {
            sellerWallet.Amount += auction.CurrentHighestBid;
            await _unitOfWork.WalletRepository.UpdateAsync(sellerWallet);
        }
    }
}