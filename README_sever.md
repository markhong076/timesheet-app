# Timesheet.Api (ASP.NET Core 8 + EF Core + PostgreSQL)

## Prereqs
- .NET 8 SDK
- Docker (for PostgreSQL) or a local PostgreSQL instance

## Run Postgres
```bash
docker compose up -d db
```

This starts a local Postgres on port 5432 (`postgres/postgres`).

## Run the API
```bash
cd server/Timesheet.Api
dotnet restore
dotnet run
```

On first run, the app auto-applies migrations. If you prefer manual control:
```bash
dotnet tool restore || dotnet new tool-manifest && dotnet tool install dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## API
- Swagger UI: http://localhost:5000/swagger
- CORS allows `http://localhost:5173` (Vite client).

**Create timesheet**
```http
POST /api/timesheets
Content-Type: application/json

{
  "description": "Week of Aug 18",
  "rate": 120.0,
  "lineItems": [
    { "date": "2025-08-18", "minutes": 90, "notes": "Kickoff" },
    { "date": "2025-08-19", "minutes": 150 }
  ]
}
```
`totalCost = (sum(minutes)/60) * rate` (rate assumed **per hour**).
