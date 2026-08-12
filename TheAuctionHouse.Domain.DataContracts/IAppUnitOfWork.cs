using TheAuctionHouse.Domain.DataContracts;

public interface IAppUnitOfWork : IDisposable
{
    IAssetRepository AssetRepository { get; }
    IAuctionRepository AuctionRepository { get; }
    IPortalUserRepository PortalUserRepository { get; }

 IWalletRepository WalletRepository { get; } 

    Task<int> SaveChangesAsync();
}