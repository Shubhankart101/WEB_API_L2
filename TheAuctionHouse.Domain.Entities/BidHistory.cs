namespace TheAuctionHouse.Domain.Entities;

public class BidHistory
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public int BidderId { get; set; }
    public string BidderName { get; set; } = string.Empty;
    public int BidAmount { get; set; }
    public DateTime BidDate { get; set; }
}