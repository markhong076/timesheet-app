# timesheet-app
- ASP.NET Core 8 minimal API with `/health`
- React + Vite + TypeScript “hello world”

## Create migration & run
```bash
docker compose up -d db
cd server/Timesheet.Api
dotnet new tool-manifest 2>/dev/null || true
dotnet tool install dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
