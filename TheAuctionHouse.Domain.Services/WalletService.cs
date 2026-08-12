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

    public Task<Result<bool>> BlockAmountAsync(WalletTransactionRequest walletTransactionRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> DepositAsync(WalletTransactionRequest walletTransactionRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WalletBalenceResponse>> GetWalletBalenceAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> WithDrawalAsync(WalletTransactionRequest walletTransactionRequest)
    {
        throw new NotImplementedException();
    }
}