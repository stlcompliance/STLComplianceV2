# TrainArr staging qualification mirror seed for k6 journeys

## Slice name

M6/M13 TrainArr load-test journey qualification mirror — idempotent issued qualification for demo subject person, GET/POST seed API, operator scripts, staging soak pre-step

## Products touched

- **TrainArr API** — `LoadTestJourneySeedService`, `POST /api/load-test-journey/seed`
- **STLCompliance.Shared** — `StlTrainArrLoadTestJourneySeedCatalog`
- **Platform ops** — `scripts/ops/trainarr-staging-journey-seed.*`
- **CI** — `load-staging-render.yml` TrainArr seed step after Compliance Core seed
- **tests** — `TrainArrLoadTestJourneySeedTests`, load catalog unit test

## API + auth changes

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/load-test-journey/seed` | `RequireTrainingDefinitionsManage` + `RequireQualificationsManage` (trainarr_admin / tenant_admin) |

Idempotently ensures for `StlLoadTestJourneyDefaults.SubjectPersonId` and `hazmat_endorsement`:

- Training definition (`load_test_journey_hazmat_endorsement`)
- Completed training assignment (`load_test_journey_seed` reason)
- Local qualification grant publication mirror
- Issued `QualificationIssue` row

Audit action: `load_test_journey.seed`

## Operator scripts

```powershell
./scripts/ops/trainarr-staging-journey-seed.ps1
```

Requires NexArr login + TrainArr handoff (same demo credentials as other load-test scripts).

## Tests

- `TrainArrLoadTestJourneySeedTests` — idempotent seed, issued qualification mirror, auth denial
- `StlRenderStagingLoadTestSupportTests.TrainArr_journey_seed_catalog_matches_load_test_defaults`

## Verification commands

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~TrainArrLoadTestJourneySeedTests"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "FullyQualifiedName~TrainArr_journey_seed_catalog"
```

## Next recommended slice

StaffArr export scheduled delivery foundations, or RoutArr staging trip mirror seed for dispatch gate k6 journeys.
