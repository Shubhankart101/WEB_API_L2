using System.ComponentModel.DataAnnotations;

public class PostAuctionRequest
{
    public int AssetId { get; set; }
    public int OwnerId { get; set; }
    public int ReservedPrice { get; set; }
    public int CurrentHighestBid { get; set; }
    public int CurrentHighestBidderId { get; set; }
    public int MinimumBidIncrement { get; set; }
    public int TotalMinutesToExpiry { get; set; }
}