using TheAuctionHouse.Common.ErrorHandling;

namespace TheAuctionHouse.Domain.ServiceContracts;

public interface IWalletService
{
    Task<Result<bool>> DepositAsync(WalletTransactionRequest walletTransactionRequest);
    Task<Result<bool>> WithDrawalAsync(WalletTransactionRequest walletTransactionRequest);
    Task<Result<bool>> BlockAmountAsync(WalletTransactionRequest walletTransactionRequest); // Block funds for bidding
    Task<Result<WalletBalenceResponse>> GetWalletBalenceAsync(int userId);
}
