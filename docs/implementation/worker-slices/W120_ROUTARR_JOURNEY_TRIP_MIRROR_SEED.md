# RoutArr staging trip mirror seed for dispatch gate k6 journeys

## Slice name

M9/M13 RoutArr load-test journey dispatch trip mirror — idempotent planned trip for demo subject person, GET/POST seed API, operator scripts, staging soak pre-step

## Products touched

- **RoutArr API** — `LoadTestJourneySeedService`, `POST /api/load-test-journey/seed`
- **STLCompliance.Shared** — `StlRoutArrLoadTestJourneySeedCatalog`
- **Platform ops** — `scripts/ops/routarr-staging-journey-seed.*`
- **CI** — `load-staging-render.yml` RoutArr seed step after TrainArr seed
- **tests** — `RoutArrLoadTestJourneySeedTests`, load catalog unit test

## API + auth changes

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/load-test-journey/seed` | `RequireTripsCreate` + `RequireTripsAssign` (routarr_admin / tenant_admin / routarr_dispatcher) |

Idempotently ensures for `StlLoadTestJourneyDefaults.SubjectPersonId`:

- Planned trip titled `Load Test Journey Dispatch Trip` with schedule window now+2h → now+6h
- Refreshes schedule when the mirrored trip window has expired

Audit action: `load_test_journey.seed`

## Operator scripts

```powershell
./scripts/ops/routarr-staging-journey-seed.ps1
```

Requires NexArr login + RoutArr handoff (same demo credentials as other load-test scripts).

## Tests

- `RoutArrLoadTestJourneySeedTests` — idempotent seed, dispatch trip mirror, auth denial
- `StlRenderStagingLoadTestSupportTests.RoutArr_journey_seed_catalog_matches_load_test_defaults`

## Verification commands

```powershell
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~RoutArrLoadTestJourneySeedTests"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "FullyQualifiedName~RoutArr_journey_seed_catalog"
dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj -c Release --filter "FullyQualifiedName~routarr"
```

## Next recommended slice

StaffArr export delivery notification hooks, or k6 optional use of `STL_LOAD_JOURNEY_TRIP_ID` from seed response.
