# M13 DR restore drill script and validation

## Slice name

M13 DR restore drill — product database catalog, post-restore validation, operator restore scripts, automated DR tests, nightly live drill.

## Products touched

- **STLCompliance.Shared** — `StlProductDatabaseCatalog`, `StlDrRestoreDrillPlan`, `StlDrRestoreDrillSupport`, `StlDrRestoreDrillValidator`
- **Platform ops** — `scripts/ops/dr-restore-drill.ps1`, `scripts/ops/dr-restore-drill.sh`
- **tests/STLCompliance.Dr.Tests** — catalog/support/validator unit tests (`Category=Dr`) + optional live NexArr restore drill (`Category=Live`)
- **CI** — `Category=Dr` step in `.github/workflows/ci.yml`; live drill in `.github/workflows/e2e-nightly.yml`

## Shared additions

| File | Purpose |
|------|---------|
| `Operations/StlProductDatabaseCatalog.cs` | Canonical seven product PostgreSQL database names |
| `Operations/StlDrRestoreDrillPlan.cs` | Operator plan (host, credentials, backup directory, drill suffix) |
| `Operations/StlDrRestoreDrillSupport.cs` | Drill DB naming, backup resolution (`.custom`/`.dump`/`.sql`), connection strings |
| `Operations/StlDrRestoreDrillValidator.cs` | Post-restore checks: connect, `__EFMigrationsHistory`, `platform_metadata` |

## Operator workflow

1. Obtain Render managed Postgres backups (or create local backups with `pg_dump -Fc`).
2. Place per-database files in a directory: `{nexarr,staffarr,...}.{custom|dump|sql}`.
3. Run the drill against staging or local Postgres:

```powershell
# Local docker-compose (pg tools via postgres container)
./scripts/ops/dr-restore-drill.ps1 `
  -BackupDirectory C:\backups\2026-05-27 `
  -DockerContainerName stlcompliancev2-postgres-1

# Staging Render Postgres (pg client on PATH)
./scripts/ops/dr-restore-drill.ps1 `
  -PostgresHost dpg-example-a.oregon-postgres.render.com `
  -PostgresPort 5432 `
  -PostgresUser nexarr `
  -PostgresPassword $env:RENDER_DB_PASSWORD `
  -BackupDirectory ./backups/render-snapshot
```

Linux/CI:

```bash
chmod +x scripts/ops/dr-restore-drill.sh
BACKUP_DIRECTORY=./backups DR_POSTGRES_CONTAINER=stlcompliancev2-postgres-1 \
  ./scripts/ops/dr-restore-drill.sh --docker-container stlcompliancev2-postgres-1 --backup-directory ./backups
```

Each database is restored to `{database}_dr_restore_drill`, validated, then dropped unless `-KeepDrillDatabases` / `--keep-drill-databases`.

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test STLCompliance.slnx -c Release --filter "Category!=Live"
dotnet test tests/STLCompliance.Dr.Tests/STLCompliance.Dr.Tests.csproj -c Release --filter "Category=Dr&Category!=Live"
```

Optional live drill (docker-compose postgres + migrated nexarr):

```powershell
docker compose up -d postgres nexarr-api
$env:DR_LIVE = "1"
dotnet test tests/STLCompliance.Dr.Tests/STLCompliance.Dr.Tests.csproj -c Release --filter "Category=Dr&Category=Live"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| DR / backup restore | **Scripted drill** — operator scripts + validation; nightly live NexArr drill |
| Load / performance | Still blocked — needs SLO definitions |

## Next slice

Load-test harness (k6/NBomber) once product owners publish SLO targets.
