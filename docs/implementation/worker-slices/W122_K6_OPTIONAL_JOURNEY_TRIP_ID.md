# k6 optional STL_LOAD_JOURNEY_TRIP_ID from RoutArr seed

## Slice name

M13 k6 RoutArr dispatch gate journey — optional `STL_LOAD_JOURNEY_TRIP_ID` reuses seeded trip mirror instead of per-iteration trip creation

## Products touched

- **STLCompliance.Shared** — `StlLoadTestJourneyDefaults.JourneyTripIdEnvVar`
- **RoutArr API** — `GET /api/load-test-journey/trip` mirror lookup
- **k6** — `resolveJourneyTripId` in `stl-journey.js`, `routarr-dispatch-workflow-gate` scenario
- **Platform ops** — `routarr-staging-journey-seed.*` exports trip id to env / `GITHUB_ENV`
- **CI** — staging soak inherits `STL_LOAD_JOURNEY_TRIP_ID` from RoutArr seed step in same workflow job

## API + auth changes

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/load-test-journey/trip` | `RequireTripsRead` | Returns seeded mirror trip id for tenant |

## k6 behavior

When `STL_LOAD_JOURNEY_TRIP_ID` is set (typically by `routarr-staging-journey-seed` after `POST /api/load-test-journey/seed`), `routarr-dispatch-workflow-gate` skips `POST /api/trips` and runs workflow gate checks against the stable trip.

When unset, behavior is unchanged (create trip per iteration).

## Operator workflow

```powershell
./scripts/ops/routarr-staging-journey-seed.ps1
# prints and sets STL_LOAD_JOURNEY_TRIP_ID
./scripts/ops/render-staging-load-soak.ps1 -Scenario routarr-dispatch-workflow-gate
```

## Tests

- `RoutArrLoadTestJourneySeedTests` — GET mirror trip after seed, 404 before seed
- `StlLoadTestJourneyDefaultsTests` — env var constant
- `StlRenderStagingLoadTestSupportTests` — RoutArr catalog + optional soak env list

## Verification commands

```powershell
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~RoutArrLoadTestJourneySeed"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "FullyQualifiedName~Journey_trip|JourneyTripId"
```

## Next recommended slice

TrainArr notification settings foundations, or Playwright assertion for shell tenant chrome after handoff.
