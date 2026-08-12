namespace TheAuctionHouse.Domain.Entities;

public class Auction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AssetId { get; set; }
    public int ReservedPrice { get; set; }
    public int CurrentHighestBid { get; set; }
    public int CurrentHighestBidderId { get; set; }
    public int MinimumBidIncrement { get; set; }
    public DateTime StartDate { get; set; }
    public int TotalMinutesToExpiry { get; set; }
    public AuctionStatus Status { get; set; }

    public int GetRemainingTimeInMinutes()
    {
        var expiryDate = StartDate.AddMinutes(TotalMinutesToExpiry);
        var remainingTime = expiryDate - DateTime.UtcNow;
        return (int)remainingTime.TotalMinutes;
    }
    public bool IsExpired()
    {
        return GetRemainingTimeInMinutes() <= 0;
    }
    public bool IsExpiredWithoutBids()
    {
        return IsExpired() && CurrentHighestBid == 0;
    }
    public bool IsLive()
    {
        return !IsExpired() && CurrentHighestBid > 0;
    }
}

public enum AuctionStatus
{
    Live,
    Expired,
    ExpiredWithoutBids
}