using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.Domain.Services;

public class PortalUserService : IPortalUserService
{
    private IAppUnitOfWork _appUnitOfWork;
    private IEmailService _emailService;
    private readonly string _jwtKey;
    public PortalUserService(IAppUnitOfWork appUnitOfWork, IEmailService emailService, string jwtKey)
    {
        _appUnitOfWork = appUnitOfWork;
        _emailService = emailService;
        _jwtKey = jwtKey;
    }

    public Task<Result<bool>> SignUpAsync(SignUpRequest signUpRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string>> LoginAsync(LoginRequest loginRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> LogoutAsync(int UserId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PortalUserResponse>> GetUserProfileAsync(int userId)
    {
        throw new NotImplementedException();
    }
}
