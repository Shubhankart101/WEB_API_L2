using System.Threading.Tasks;
using Moq;
using Xunit;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Common.ErrorHandling;

namespace TheAuctionHouse.Domain.Services.Tests;
public class WalletServiceTests
{
    private readonly Mock<IAppUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IWalletRepository> _walletRepoMock;
    private readonly Mock<IAuctionRepository> _auctionRepoMock;
    private readonly WalletService _service;

    public WalletServiceTests()
    {
        _unitOfWorkMock = new Mock<IAppUnitOfWork>();
        _walletRepoMock = new Mock<IWalletRepository>();
        _auctionRepoMock = new Mock<IAuctionRepository>();
        _unitOfWorkMock.Setup(u => u.WalletRepository).Returns(_walletRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.AuctionRepository).Returns(_auctionRepoMock.Object);
        _service = new WalletService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task DepositAsync_WithInvalidAmount_ReturnsValidationError()
    {
        var request = new WalletTransactionRequest { UserId = 1, Amount = 0 };
        var result = await _service.DepositAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.ErrorCode);
    }

    [Fact]
    public async Task DepositAsync_NewWallet_CreatesWallet()
    {
        var request = new WalletTransactionRequest { UserId = 1, Amount = 1000 };
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((Wallet?)null);

        var result = await _service.DepositAsync(request);

        Assert.True(result.IsSuccess);
        _walletRepoMock.Verify(r => r.AddAsync(It.Is<Wallet>(w => w.UserId == 1 && w.Amount == 1000)), Times.Once);
    }

    [Fact]
    public async Task DepositAsync_ExistingWallet_UpdatesAmount()
    {
        var wallet = new Wallet { UserId = 1, Amount = 500, BlockedAmount = 0 };
        var request = new WalletTransactionRequest { UserId = 1, Amount = 1000 };
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(wallet);

        var result = await _service.DepositAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1500, wallet.Amount);
        _walletRepoMock.Verify(r => r.UpdateAsync(wallet), Times.Once);
    }

    [Fact]
    public async Task WithDrawalAsync_WithInvalidAmount_ReturnsValidationError()
    {
        var request = new WalletTransactionRequest { UserId = 1, Amount = -10 };
        var result = await _service.WithDrawalAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.ErrorCode);
    }

    [Fact]
    public async Task WithDrawalAsync_WalletNotFound_ReturnsNotFound()
    {
        var request = new WalletTransactionRequest { UserId = 1, Amount = 100 };
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((Wallet?)null);

        var result = await _service.WithDrawalAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error.ErrorCode);
    }

    [Fact]
    public async Task WithDrawalAsync_InsufficientBalance_ReturnsError()
    {
        var wallet = new Wallet { UserId = 1, Amount = 100, BlockedAmount = 50 };
        var request = new WalletTransactionRequest { UserId = 1, Amount = 60 };
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(wallet);

        var result = await _service.WithDrawalAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.ErrorCode);
        Assert.Contains("Insufficient", result.Error.Message);
    }

    [Fact]
    public async Task WithDrawalAsync_SufficientBalance_Withdraws()
    {
        var wallet = new Wallet { UserId = 1, Amount = 200, BlockedAmount = 50 };
        var request = new WalletTransactionRequest { UserId = 1, Amount = 100 };
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(wallet);

        var result = await _service.WithDrawalAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, wallet.Amount);
        _walletRepoMock.Verify(r => r.UpdateAsync(wallet), Times.Once);
    }

    [Fact]
    public async Task GetWalletBalenceAsync_WalletNotFound_ReturnsNotFound()
    {
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((Wallet?)null);

        var result = await _service.GetWalletBalenceAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error.ErrorCode);
    }

    [Fact]
    public async Task GetWalletBalenceAsync_WalletFound_ReturnsBalance()
    {
        var wallet = new Wallet { UserId = 1, Amount = 500, BlockedAmount = 100 };
        _walletRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(wallet);
        // Mock bid histories to avoid NullReferenceException
        _auctionRepoMock.Setup(r => r.GetBidHistoriesByUserIdAsync(1)).ReturnsAsync(new List<BidHistory>());

        var result = await _service.GetWalletBalenceAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(500, result.Value.Amount);
        Assert.Equal(100, result.Value.BlockedAmount);
    }
}