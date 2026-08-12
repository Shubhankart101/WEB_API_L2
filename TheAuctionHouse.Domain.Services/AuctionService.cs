using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;

public class AuctionService : IAuctionService
{
    private readonly IAppUnitOfWork _unitOfWork;

    public AuctionService(IAppUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<Result<bool>> CheckAuctionExpiriesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<AuctionResponse>>> GetAllOpenAuctionsByUserIdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<AuctionResponse>> GetAuctionByIdAsync(int auctionId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<AuctionResponse>>> GetAuctionsByUserIdAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> PlaceBidAsync(PlaceBidRequest placeBidRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> PostAuctionAsync(PostAuctionRequest postAuctionRequest)
    {
        throw new NotImplementedException();
    }
}