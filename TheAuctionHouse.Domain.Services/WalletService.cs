using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;

namespace TheAuctionHouse.Domain.Services;

public class WalletService : IWalletService
{
    private readonly IAppUnitOfWork _unitOfWork;

    public WalletService(IAppUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> BlockAmountAsync(WalletTransactionRequest walletTransactionRequest)
    {
        var validationError = ValidateAmount(walletTransactionRequest);
        if (validationError is not null)
        {
            return validationError;
        }

        var wallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(walletTransactionRequest.UserId);
        if (wallet is null)
        {
            return Error.NotFound("Wallet not found.");
        }

        var availableBalance = wallet.Amount - wallet.BlockedAmount;
        if (availableBalance < walletTransactionRequest.Amount)
        {
            return Error.BadRequest("Insufficient available balance to block amount.");
        }

        wallet.BlockedAmount += walletTransactionRequest.Amount;
        await _unitOfWork.WalletRepository.UpdateAsync(wallet);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Result<bool>> DepositAsync(WalletTransactionRequest walletTransactionRequest)
    {
        var validationError = ValidateAmount(walletTransactionRequest);
        if (validationError is not null)
        {
            return validationError;
        }

        var wallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(walletTransactionRequest.UserId);
        if (wallet is null)
        {
            wallet = new Wallet
            {
                UserId = walletTransactionRequest.UserId,
                Amount = walletTransactionRequest.Amount,
                BlockedAmount = 0
            };

            await _unitOfWork.WalletRepository.AddAsync(wallet);
        }
        else
        {
            wallet.Amount += walletTransactionRequest.Amount;
            await _unitOfWork.WalletRepository.UpdateAsync(wallet);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Result<WalletBalenceResponse>> GetWalletBalenceAsync(int userId)
    {
        var wallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            return Error.NotFound("Wallet not found.");
        }

        var bidHistories = await _unitOfWork.AuctionRepository.GetBidHistoriesByUserIdAsync(userId);
        var response = new WalletBalenceResponse
        {
            UserId = wallet.UserId,
            Amount = wallet.Amount,
            BlockedAmount = wallet.BlockedAmount,
            BidHistory = bidHistories
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

        return response;
    }

    public async Task<Result<bool>> WithDrawalAsync(WalletTransactionRequest walletTransactionRequest)
    {
        var validationError = ValidateAmount(walletTransactionRequest);
        if (validationError is not null)
        {
            return validationError;
        }

        var wallet = await _unitOfWork.WalletRepository.GetByUserIdAsync(walletTransactionRequest.UserId);
        if (wallet is null)
        {
            return Error.NotFound("Wallet not found.");
        }

        var availableBalance = wallet.Amount - wallet.BlockedAmount;
        if (availableBalance < walletTransactionRequest.Amount)
        {
            return Error.BadRequest("Insufficient available balance.");
        }

        wallet.Amount -= walletTransactionRequest.Amount;
        await _unitOfWork.WalletRepository.UpdateAsync(wallet);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static Error? ValidateAmount(WalletTransactionRequest walletTransactionRequest)
    {
        if (walletTransactionRequest.UserId <= 0)
        {
            return Error.BadRequest("UserId must be greater than zero.");
        }

        if (walletTransactionRequest.Amount <= 0)
        {
            return Error.BadRequest("Amount must be greater than zero.");
        }

        if (walletTransactionRequest.Amount > 999999)
        {
            return Error.BadRequest("Amount cannot exceed 999999.");
        }

        return null;
    }
}