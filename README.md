# The Auction House

The Auction House is a .NET 8 solution for an online auction platform. The codebase is organized around domain entities, service contracts, service implementations, an EF Core in-memory data layer, and a test project that exercises the business rules.

## Solution Structure

- `TheAuctionHouse.Common`: shared result, error, validation, and cross-cutting abstractions.
- `TheAuctionHouse.Domain.Entities`: core auction, asset, wallet, and user entities.
- `TheAuctionHouse.Domain.DataContracts`: repository and unit-of-work interfaces.
- `TheAuctionHouse.Domain.ServiceContracts`: DTOs and service interfaces.
- `TheAuctionHouse.Domain.Services`: domain service implementations.
- `TheAuctionHouse.Data.EFCore.InMemory`: EF Core in-memory persistence and repositories.
- `TheAuctionHouse.Domain.Services.Tests`: automated tests for the service layer.

## Requirements

- .NET SDK 8.0
- Visual Studio 2022 or the `dotnet` CLI

## Build

```powershell
dotnet build .\TheAuctionHouse.sln
```

## Test

```powershell
dotnet test .\TheAuctionHouse.sln
```

## Domain Summary

Based on the requirements in `SRS.md`, the platform supports:

- user registration and profile management
- wallet deposits and withdrawals
- asset creation, updates, and ownership transfer
- auction posting, bidding, and expiry handling
- service-layer business rules around bidding and wallet balance checks

## Notes

- The persistence project uses EF Core's in-memory provider, which is suitable for development and test scenarios.
- The repository currently contains the solution, supporting class libraries, and service-layer tests.