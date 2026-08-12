namespace TheAuctionHouse.Domain.Entities;

public class PortalUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmailId { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public int WalletBalence { get; set; }
    public int WalletBalenceBlocked { get; set; }
}
