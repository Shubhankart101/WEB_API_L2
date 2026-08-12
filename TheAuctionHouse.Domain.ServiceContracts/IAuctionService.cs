using TheAuctionHouse.Common.ErrorHandling;
public interface IAuctionService
{
    Task<Result<bool>> PostAuctionAsync(PostAuctionRequest postAuctionRequest);
    Task<Result<bool>> CheckAuctionExpiriesAsync();
    Task<Result<AuctionResponse>> GetAuctionByIdAsync(int auctionId);
    Task<Result<List<AuctionResponse>>> GetAuctionsByUserIdAsync(int userId);
    Task<Result<List<AuctionResponse>>> GetAllOpenAuctionsByUserIdAsync();
    Task<Result<bool>> PlaceBidAsync(PlaceBidRequest placeBidRequest);
}