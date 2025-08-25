# Timesheet App (ASP.NET Core + React + PostgreSQL)

## Quick Start

1) Start PostgreSQL (via Docker):
```bash
docker compose up -d db
```

2) Run the API:
```bash
cd server/Timesheet.Api
dotnet restore
dotnet run
```
- Swagger at: http://localhost:5000/swagger

3) Run the client:
```bash
cd client
npm i
npm run dev
```
- Visit http://localhost:5173

The client assumes the API at `http://localhost:5000`. To change, create `client/.env` with:
```
VITE_API_BASE=http://localhost:5000
```

### Notes
- Entity class is **TimesheetRecord**. Each record has many **TimesheetLineItem** entries.
- `totalMinutes` is the sum of line item minutes. `totalCost = (totalMinutes / 60) * rate` (rate per hour).
- CORS is configured for the Vite dev server.
- EF Core migrations auto-apply on app start; you can switch to manual if preferred.

### TODO
- Create user table
- Create Authentication/Authorization (I'm thinking JWT)
- Host on AWS
- Add testing suite
- Add some styling (thinking Tailwind)
