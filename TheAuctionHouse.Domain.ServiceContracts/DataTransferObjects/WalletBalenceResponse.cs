public class WalletBalenceResponse
{
    public int UserId { get; set; }
    public int Amount { get; set; }
    public int BlockedAmount { get; set; }
    public List<BidHistoryResponse> BidHistory { get; set; } = new List<BidHistoryResponse>();
}