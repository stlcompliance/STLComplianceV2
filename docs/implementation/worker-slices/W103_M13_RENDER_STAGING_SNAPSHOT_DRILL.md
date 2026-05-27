# M13 Render staging snapshot DR drill

## Slice name

M13 Render staging snapshot DR drill — operator scripts to pg_dump Render staging Postgres, restore to drill databases, validate, optional GitHub workflow_dispatch, shared catalog/plan helpers, automated DR tests.

## Products touched

- **STLCompliance.Shared** — `StlRenderStagingDrillCatalog`, `StlRenderStagingDatabaseTarget`, `StlRenderStagingDrillPlan`, `StlRenderStagingDrillSupport`
- **Platform ops** — `scripts/ops/render-staging-snapshot-fetch.*`, `scripts/ops/render-staging-dr-restore-drill.*` (wraps `dr-restore-drill.*`)
- **tests/STLCompliance.Dr.Tests** — staging catalog/support unit tests (`Category=Dr`) + optional live staging drill (`Category=Live`, `DR_RENDER_STAGING_LIVE=1`)
- **CI** — `.github/workflows/dr-staging-render.yml` (manual workflow_dispatch; requires GitHub secrets)

## Environment variables

| Variable | Purpose |
|----------|---------|
| `RENDER_STAGING_NEXARR_DATABASE_URL` | External Postgres URI for NexArr staging |
| `RENDER_STAGING_STAFFARR_DATABASE_URL` | StaffArr staging |
| `RENDER_STAGING_TRAINARR_DATABASE_URL` | TrainArr staging |
| `RENDER_STAGING_MAINTAINARR_DATABASE_URL` | MaintainArr staging |
| `RENDER_STAGING_ROUTARR_DATABASE_URL` | RoutArr staging |
| `RENDER_STAGING_SUPPLYARR_DATABASE_URL` | SupplyArr staging |
| `RENDER_STAGING_COMPLIANCECORE_DATABASE_URL` | Compliance Core staging |
| `RENDER_STAGING_SNAPSHOT_DIRECTORY` | Backup output/input directory (optional) |
| `DR_RENDER_STAGING_LIVE` | Set to `1` for live xUnit staging drill |

Each URL is a standard `postgresql://user:pass@host:port/db` external connection string from the Render Dashboard (managed Postgres → Connections → External).

## Operator workflow

```powershell
# 1. Export staging external URLs (one per Render database)
$env:RENDER_STAGING_NEXARR_DATABASE_URL = "postgresql://..."
# ... repeat for all seven ...

# 2. Full drill (fetch snapshots + restore + validate + cleanup)
./scripts/ops/render-staging-dr-restore-drill.ps1

# Or use pre-downloaded Render dashboard backups
./scripts/ops/render-staging-dr-restore-drill.ps1 `
  -BackupDirectory C:\backups\render-staging `
  -SkipSnapshotFetch
```

Linux / GitHub Actions:

```bash
export RENDER_STAGING_NEXARR_DATABASE_URL="postgresql://..."
./scripts/ops/render-staging-dr-restore-drill.sh --backup-directory ./backups
```

GitHub: run workflow **DR Staging Render** after configuring repository secrets matching the environment variables above.

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test STLCompliance.slnx -c Release --filter "Category!=Live"
dotnet test tests/STLCompliance.Dr.Tests/STLCompliance.Dr.Tests.csproj -c Release --filter "Category=Dr&Category!=Live"
```

Optional live staging drill (requires pg tools on PATH + all seven staging URLs):

```powershell
$env:DR_RENDER_STAGING_LIVE = "1"
dotnet test tests/STLCompliance.Dr.Tests/STLCompliance.Dr.Tests.csproj -c Release --filter "FullyQualifiedName~RenderStagingDrillLiveTests"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| DR / backup restore | **Staging operator drill** — fetch + restore against Render managed Postgres; nightly docker-compose drill unchanged (W102) |
| Load / performance | Still blocked — needs PO SLO document |

## Next slice

Product-owner SLO adoption (unblocks authenticated k6 flows) once PO publishes SLO targets.
