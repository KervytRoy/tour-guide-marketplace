# Tour Guide Marketplace

A tourism services platform where local guides and tourists connect to offer, book, and manage guided experiences.

## Backend MVP Base

This branch starts the .NET 10 API foundation as a modular monolith with clean boundaries:

- `TourGuideMarketplace.Api`: HTTP boundary, controllers, JWT bearer configuration and OpenAPI.
- `TourGuideMarketplace.Application`: DTOs, contracts, result models and shared security constants.
- `TourGuideMarketplace.Domain`: business entities for guides, tourists, bookings, payments and reviews.
- `TourGuideMarketplace.Infrastructure`: SQL Server EF Core persistence, ASP.NET Core Identity, JWT issuing and application service implementations.

## Implemented Capabilities

- Registration/login with ASP.NET Core Identity.
- JWT access tokens and hashed refresh tokens.
- Roles: `Tourist`, `Guide`, `Admin`.
- Current-user endpoint.
- Guide profile upsert for authenticated guides.
- Guide search with filters for city, country, specialty, language, rate, rating and immediate availability.
- EF Core Code First model and initial SQL Server migration.
- Local `dotnet-ef` tool manifest.

## Useful Commands

```powershell
dotnet tool restore
dotnet build TourGuideMarketplace.slnx
dotnet tool run dotnet-ef database update --project src\TourGuideMarketplace.Infrastructure\TourGuideMarketplace.Infrastructure.csproj --startup-project src\TourGuideMarketplace.Api\TourGuideMarketplace.Api.csproj
dotnet run --project src\TourGuideMarketplace.Api\TourGuideMarketplace.Api.csproj --launch-profile http
```

The default development connection string uses SQL Server LocalDB:

```json
"Server=(localdb)\\MSSQLLocalDB;Database=TourGuideMarketplace;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Replace the development JWT secret before using this outside local development.
