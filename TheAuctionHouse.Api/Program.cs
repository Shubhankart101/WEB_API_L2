using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Data.EFCore.InMemory;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The Auction House API",
        Version = "v1",
        Description = "Minimal API for asset registration, auctions, bidding, wallet operations, and JWT authentication."
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Paste a JWT bearer token. Example: Bearer {token}",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [jwtSecurityScheme] = Array.Empty<string>()
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key configuration is missing.");
var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
        };
    });

builder.Services.AddAuthorization();

var persistenceProvider = builder.Configuration["Persistence:Provider"];
if (string.Equals(persistenceProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
{
    var databaseName = builder.Configuration.GetConnectionString("InMemory") ?? "TheAuctionHouse";
    builder.Services.AddDbContext<InMemoryAppDbContext>(options => options.UseInMemoryDatabase(databaseName));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=auctionhouse.db";
    builder.Services.AddDbContext<InMemoryAppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddScoped<IAppUnitOfWork, InMemoryAppUnitOfWork>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPortalUserService>(serviceProvider =>
    new PortalUserService(
        serviceProvider.GetRequiredService<IAppUnitOfWork>(),
        serviceProvider.GetRequiredService<IEmailService>(),
        jwtKey));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InMemoryAppDbContext>();
    if (string.Equals(persistenceProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
    {
        dbContext.Database.EnsureCreated();
    }
    else
    {
        dbContext.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "The Auction House API Docs";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "The Auction House API v1");
        options.DisplayRequestDuration();
    });
}

app.UseAuthentication();
app.UseAuthorization();

var authGroup = app.MapGroup("/api/auth");
authGroup.MapPost("/signup", async (SignUpRequest request, IPortalUserService service) => ToHttpResult(await service.SignUpAsync(request)));
authGroup.MapPost("/login", async (LoginRequest request, IPortalUserService service) => ToHttpResult(await service.LoginAsync(request)));
authGroup.MapPost("/forgot-password", async (ForgotPasswordRequest request, IPortalUserService service) => ToHttpResult(await service.ForgotPasswordAsync(request)));
authGroup.MapPost("/reset-password", async (ResetPasswordRequest request, ClaimsPrincipal user, IPortalUserService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    request.UserId = userId.Value;
    return ToHttpResult(await service.ResetPasswordAsync(request));
}).RequireAuthorization();

var userGroup = app.MapGroup("/api/users").RequireAuthorization();
userGroup.MapGet("/me", async (ClaimsPrincipal user, IPortalUserService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    return ToHttpResult(await service.GetUserProfileAsync(userId.Value));
});

var assetGroup = app.MapGroup("/api/assets").RequireAuthorization();
assetGroup.MapPost("/", async (AssetInformationUpdateRequest request, ClaimsPrincipal user, IAssetService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    return ToHttpResult(await service.CreateAssetAsync(request, userId.Value));
});
assetGroup.MapPut("/{assetId:int}", async (int assetId, AssetInformationUpdateRequest request, ClaimsPrincipal user, IAssetService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    request.AssetId = assetId;
    request.UserId = userId.Value;
    return ToHttpResult(await service.UpdateAssetAsync(request));
});
assetGroup.MapDelete("/{assetId:int}", async (int assetId, IAssetService service) => ToHttpResult(await service.DeleteAssetAsync(assetId)));
assetGroup.MapGet("/my", async (ClaimsPrincipal user, IAssetService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    return ToHttpResult(await service.GetAllAssetsByUserIdAsync(userId.Value));
});
assetGroup.MapGet("/{assetId:int}", async (int assetId, IAssetService service) => ToHttpResult(await service.GetAssetByIdAsync(assetId)));

var auctionGroup = app.MapGroup("/api/auctions");
auctionGroup.MapGet("/open", async (IAuctionService service) => ToHttpResult(await service.GetAllOpenAuctionsByUserIdAsync()));
auctionGroup.MapGet("/{auctionId:int}", async (int auctionId, IAuctionService service) => ToHttpResult(await service.GetAuctionByIdAsync(auctionId)));
auctionGroup.MapGet("/my", async (ClaimsPrincipal user, IAuctionService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    return ToHttpResult(await service.GetAuctionsByUserIdAsync(userId.Value));
}).RequireAuthorization();
auctionGroup.MapPost("/", async (PostAuctionRequest request, ClaimsPrincipal user, IAuctionService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    request.OwnerId = userId.Value;
    return ToHttpResult(await service.PostAuctionAsync(request));
}).RequireAuthorization();
auctionGroup.MapPost("/{auctionId:int}/bids", async (int auctionId, PlaceBidRequest request, ClaimsPrincipal user, IAuctionService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    request.AuctionId = auctionId;
    request.UserId = userId.Value;
    return ToHttpResult(await service.PlaceBidAsync(request));
}).RequireAuthorization();
auctionGroup.MapPost("/check-expiries", async (IAuctionService service) => ToHttpResult(await service.CheckAuctionExpiriesAsync())).RequireAuthorization();

var walletGroup = app.MapGroup("/api/wallet").RequireAuthorization();
walletGroup.MapPost("/deposit", async (WalletTransactionRequest request, ClaimsPrincipal user, IWalletService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    request.UserId = userId.Value;
    return ToHttpResult(await service.DepositAsync(request));
});
walletGroup.MapPost("/withdraw", async (WalletTransactionRequest request, ClaimsPrincipal user, IWalletService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    request.UserId = userId.Value;
    return ToHttpResult(await service.WithDrawalAsync(request));
});
walletGroup.MapGet("/me", async (ClaimsPrincipal user, IWalletService service) =>
{
    var userId = GetUserId(user);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    return ToHttpResult(await service.GetWalletBalenceAsync(userId.Value));
});

app.Run();

static int? GetUserId(ClaimsPrincipal user)
{
    var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
    return int.TryParse(value, out var userId) ? userId : null;
}

static IResult ToHttpResult<T>(Result<T> result)
{
    if (result.IsSuccess)
    {
        return Results.Ok(result.Value);
    }

    return result.Error.ErrorCode switch
    {
        400 => Results.BadRequest(result.Error),
        401 => Results.Unauthorized(),
        404 => Results.NotFound(result.Error),
        422 => Results.UnprocessableEntity(result.Error),
        _ => Results.Problem(result.Error.Message, statusCode: result.Error.ErrorCode)
    };
}