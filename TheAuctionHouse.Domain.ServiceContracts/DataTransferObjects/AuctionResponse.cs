public class AuctionResponse
{
    public int AuctionId { get; set; }
    public int UserId { get; set; }
    public int AssetId { get; set; }
    public string AssetTitle { get; set; } = string.Empty;
    public string AssetDescription { get; set; } = string.Empty;
    public int CurrentHighestBid { get; set; }
    public int CurrentHighestBidderId { get; set; }
    public string HighestBidderName { get; set; } = string.Empty;
    public int MinimumBidIncrement { get; set; }
    public int ReservedPrice { get; set; } // Added ReservedPrice property

    public int CallFor
    {
        get
        {
            return CurrentHighestBid + MinimumBidIncrement;
        }
    }
    public DateTime StartDate { get; set; }
    public int TotalMinutesToExpiry { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CustomStatusMessage { get; set; } // Property for custom status message
    public List<BidHistoryResponse> BidHistory { get; set; } = new List<BidHistoryResponse>();
}