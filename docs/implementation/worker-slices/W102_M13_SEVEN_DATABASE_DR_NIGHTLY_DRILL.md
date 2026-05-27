# M13 Seven-database DR nightly drill

## Slice name

M13 seven-database DR nightly drill — extend live restore validation to all product PostgreSQL databases and run in nightly CI.

## Products touched

- **Platform** — `tests/STLCompliance.Dr.Tests` live drill per `StlProductDatabaseCatalog.All`
- **CI** — `.github/workflows/e2e-nightly.yml` live DR step (seven databases)

## Implementation

| Area | Change |
|------|--------|
| `DrRestoreDrillLiveRunner.cs` | Shared backup → drill DB restore → `StlDrRestoreDrillValidator` for one product database |
| `DrRestoreDrillLiveTests.cs` | `[Theory]` over all seven databases (replaces NexArr-only live test) |
| `e2e-nightly.yml` | Step documents seven-database live drill (same filter, seven test cases) |

Operator scripts (`scripts/ops/dr-restore-drill.*`) already supported all seven databases from W99; this slice closes the automated nightly gap.

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test STLCompliance.slnx -c Release --filter "Category!=Live"

dotnet test tests/STLCompliance.Dr.Tests/STLCompliance.Dr.Tests.csproj -c Release --filter "Category=Dr&Category!=Live"

# Optional live (docker-compose postgres + all APIs for migrations)
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
$env:DR_LIVE = "1"
dotnet test tests/STLCompliance.Dr.Tests/STLCompliance.Dr.Tests.csproj -c Release --filter "Category=Dr&Category=Live"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| DR / backup restore | **Nightly live drill** — all seven product DBs on docker-compose; operator scripts unchanged |
| Load / performance | Still blocked — needs PO SLO document |

## Next slice

Product-owner SLO adoption (unblocks authenticated k6 flows) or staging Render snapshot drill using operator scripts against managed Postgres backups.
