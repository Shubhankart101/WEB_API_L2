using Moq;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.Services;

namespace TheAuctionHouse.Domain.Services.Tests;

public class PortalUserServiceTests
{
    [Fact]
    public async Task SignUpAsync_WithValidRequest_CreatesUserAndWallet()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        var emailService = new Mock<IEmailService>();
        var service = new PortalUserService(unitOfWork, emailService.Object, "test-jwt-key-12345678901234567890");

        var result = await service.SignUpAsync(new SignUpRequest
        {
            Name = "Valid User",
            EmailId = "valid@domain.com",
            Password = "Password1"
        });

        Assert.True(result.IsSuccess);

        var user = await unitOfWork.PortalUserRepository.GetUserByEmailAsync("valid@domain.com");
        var wallet = await unitOfWork.WalletRepository.GetByUserIdAsync(user!.Id);

        Assert.NotNull(user);
        Assert.NotEqual("Password1", user.HashedPassword);
        Assert.NotNull(wallet);
    }

    [Fact]
    public async Task SignUpAsync_WithDuplicateEmail_ReturnsBadRequest()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        await unitOfWork.PortalUserRepository.AddAsync(new PortalUser
        {
            Name = "Existing User",
            EmailId = "existing@domain.com",
            HashedPassword = "hashed-password"
        });
        await unitOfWork.SaveChangesAsync();

        var service = new PortalUserService(unitOfWork, new Mock<IEmailService>().Object, "test-jwt-key-12345678901234567890");
        var result = await service.SignUpAsync(new SignUpRequest
        {
            Name = "Another User",
            EmailId = "existing@domain.com",
            Password = "Password1"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        var service = new PortalUserService(unitOfWork, new Mock<IEmailService>().Object, "test-jwt-key-12345678901234567890");
        await service.SignUpAsync(new SignUpRequest
        {
            Name = "Login User",
            EmailId = "login@domain.com",
            Password = "Password1"
        });

        var result = await service.LoginAsync(new LoginRequest
        {
            EmailId = "login@domain.com",
            Password = "Password1"
        });

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value));
    }

    [Fact]
    public async Task ResetPasswordAsync_WithWrongOldPassword_ReturnsBadRequest()
    {
        using var unitOfWork = TestAppUnitOfWorkFactory.Create();
        var service = new PortalUserService(unitOfWork, new Mock<IEmailService>().Object, "test-jwt-key-12345678901234567890");
        await service.SignUpAsync(new SignUpRequest
        {
            Name = "Reset User",
            EmailId = "reset@domain.com",
            Password = "Password1"
        });

        var user = await unitOfWork.PortalUserRepository.GetUserByEmailAsync("reset@domain.com");
        var result = await service.ResetPasswordAsync(new ResetPasswordRequest
        {
            UserId = user!.Id,
            OldPassword = "WrongPassword",
            NewPassword = "Password2",
            ConfirmPassword = "Password2"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Error.ErrorCode);
    }
}