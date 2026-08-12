using System;

    public class BidHistoryResponse
    {
        public int BidId { get; set; }
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public decimal BidAmount { get; set; }
        public DateTime BidTime { get; set; }
        public string UserName { get; set; } = string.Empty;
    }