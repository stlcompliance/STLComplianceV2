# STL Compliance / Arr Suite

Greenfield monorepo for the full V1 STL Compliance / Arr product suite (.NET 10 APIs, background workers, PostgreSQL per product, Render deployment).

## Milestone status

| Milestone | Status |
|-----------|--------|
| M1 — Render & repo foundation | Complete |
| M2 — NexArr platform access spine | Partial |
| M4 — StaffArr workforce spine | In progress (see `docs/implementation-status.md`) |

## Repository layout

```txt
apps/           # Product APIs (nexarr, staffarr, trainarr, maintainarr, routarr, supplyarr, compliancecore)
workers/        # Product background workers
packages/       # shared-dotnet (hosting, health, tenant-scoped EF foundation)
tests/          # Cross-suite tests
docs/           # Masterplan, featuresets, implementation milestones
docker/         # Shared Docker build patterns
docker-compose.yml
render.yaml
```

## Local development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Postgres, Redis, containerized APIs)

### Run APIs directly (dotnet)

```powershell
dotnet run --project apps/nexarr-api/NexArr.Api/NexArr.Api.csproj
# http://localhost:5101/health
```

| API | Port |
|-----|------|
| NexArr | 5101 |
| StaffArr | 5102 |
| TrainArr | 5103 |
| MaintainArr | 5104 |
| RoutArr | 5105 |
| SupplyArr | 5106 |
| Compliance Core | 5107 |

Start Postgres locally (or `docker compose up postgres -d`) and use connection strings in each API's `appsettings.Development.json`.

### Run full stack (Docker Compose)

```powershell
docker compose up --build
```

## Health endpoints

Every product API exposes:

- `GET /health` — liveness (no database required)
- `GET /health/ready` — readiness (includes PostgreSQL when `DATABASE_URL` / `ConnectionStrings:Database` is set)

## Database migrations

Each product API owns its PostgreSQL database and EF Core migrations under `apps/<product>-api/<Product>.Api/Migrations/`.

```powershell
dotnet ef database update --project apps/nexarr-api/NexArr.Api/NexArr.Api.csproj
```

## CI

GitHub Actions workflow: `.github/workflows/ci.yml` — restore, build, test.

## Deployment

`render.yaml` defines databases, Redis, Docker web services, and workers for Render Blueprint sync.

## Documentation

See `docs/README_INDEX.md` and `docs/implementation/01_MILESTONE_MASTERPLAN.md`.
