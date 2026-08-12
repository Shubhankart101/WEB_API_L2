using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Domain.DataContracts;
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

    public async Task<Result<bool>> SignUpAsync(SignUpRequest signUpRequest)
    {
        var validationError = ValidateSignUpRequest(signUpRequest);
        if (validationError is not null)
        {
            return validationError;
        }

        var existingUser = await _appUnitOfWork.PortalUserRepository.GetUserByEmailAsync(signUpRequest.EmailId.Trim());
        if (existingUser is not null)
        {
            return Error.BadRequest("Email is already registered.");
        }

        var portalUser = new PortalUser
        {
            Name = signUpRequest.Name.Trim(),
            EmailId = signUpRequest.EmailId.Trim(),
            HashedPassword = HashPassword(signUpRequest.Password),
            WalletBalence = 0,
            WalletBalenceBlocked = 0
        };

        await _appUnitOfWork.PortalUserRepository.AddAsync(portalUser);
        await _appUnitOfWork.SaveChangesAsync();

        await _appUnitOfWork.WalletRepository.AddAsync(new Wallet
        {
            UserId = portalUser.Id,
            Amount = 0,
            BlockedAmount = 0
        });
        await _appUnitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Result<string>> LoginAsync(LoginRequest loginRequest)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.EmailId) || string.IsNullOrWhiteSpace(loginRequest.Password))
        {
            return Error.BadRequest("Email and password are required.");
        }

        var user = await _appUnitOfWork.PortalUserRepository.GetUserByEmailAsync(loginRequest.EmailId.Trim());
        if (user is null || !PasswordsMatch(user.HashedPassword, loginRequest.Password))
        {
            return Error.BadRequest("Invalid email or password.");
        }

        return GenerateJwtToken(user);
    }

    public async Task<Result<bool>> LogoutAsync(int UserId)
    {
        var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(UserId);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

        return true;
    }

    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
    {
        var validationError = Error.ValidationFailures();
        if (!ValidationHelper.Validate(forgotPasswordRequest, validationError))
        {
            return validationError;
        }

        var user = await _appUnitOfWork.PortalUserRepository.GetUserByEmailAsync(forgotPasswordRequest.EmailId.Trim());
        if (user is null)
        {
            return Error.NotFound("Email address is not registered.");
        }

        await _emailService.SendEmailAsync(user.EmailId, "Password Reset | The Auction House", string.Empty, true);
        return true;
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
    {
        if (resetPasswordRequest.UserId <= 0)
        {
            return Error.BadRequest("UserId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword) || resetPasswordRequest.NewPassword.Length < 6)
        {
            return Error.BadRequest("New password must be at least 6 characters long.");
        }

        if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmPassword)
        {
            return Error.BadRequest("New password and confirm password must match.");
        }

        var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(resetPasswordRequest.UserId);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

        if (!PasswordsMatch(user.HashedPassword, resetPasswordRequest.OldPassword))
        {
            return Error.BadRequest("Old password is incorrect.");
        }

        user.HashedPassword = HashPassword(resetPasswordRequest.NewPassword);
        await _appUnitOfWork.PortalUserRepository.UpdateAsync(user);
        await _appUnitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Result<PortalUserResponse>> GetUserProfileAsync(int userId)
    {
        var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(userId);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

        return new PortalUserResponse
        {
            UserId = user.Id,
            Name = user.Name,
            EmailId = user.EmailId
        };
    }

    private static Error? ValidateSignUpRequest(SignUpRequest signUpRequest)
    {
        if (string.IsNullOrWhiteSpace(signUpRequest.Name) || signUpRequest.Name.Trim().Length < 3)
        {
            return Error.BadRequest("Name must be at least 3 characters long.");
        }

        if (string.IsNullOrWhiteSpace(signUpRequest.EmailId) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(signUpRequest.EmailId.Trim()))
        {
            return Error.BadRequest("A valid email address is required.");
        }

        if (string.IsNullOrWhiteSpace(signUpRequest.Password) || signUpRequest.Password.Length < 6)
        {
            return Error.BadRequest("Password must be at least 6 characters long.");
        }

        return null;
    }

    private string GenerateJwtToken(PortalUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.EmailId)
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool PasswordsMatch(string storedPassword, string providedPassword)
    {
        return storedPassword == providedPassword || storedPassword == HashPassword(providedPassword);
    }
}
