# STLCompliance.OpenApi.Tests

M13 OpenAPI parity gate — verifies each suite API exposes a stable `/openapi/v1.json` document that matches checked-in snapshots.

## What it checks

For all seven product APIs (NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core):

1. **Snapshot parity** — normalized OpenAPI JSON matches `snapshots/{product}.openapi.json`
2. **Minimum coverage** — document includes `/health` and at least one `/api/` route

Normalization strips volatile fields (e.g. `info.version`) and sorts paths for stable diffs.

## CI

Runs automatically in GitHub Actions via the `OpenAPI parity` step and the main `dotnet test` job (`Category!=Live` includes `Category=OpenApi`).

```powershell
dotnet test "tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj" -c Release --filter "Category=OpenApi"
```

## Updating snapshots

When routes or schemas change intentionally, regenerate baselines locally and commit the updated JSON files:

```powershell
$env:OPENAPI_UPDATE_SNAPSHOTS = "1"
dotnet test "tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj" -c Release --filter "Category=OpenApi"
git add tests/STLCompliance.OpenApi.Tests/snapshots/
```

## Host configuration

OpenAPI is mapped in **Development** and **Testing** environments (`StlApiHost`). Production does not expose `/openapi/v1.json`; snapshots are generated from in-memory `WebApplicationFactory` hosts with no database.

See also: `docs/implementation/worker-slices/W92_M13_OPENAPI_PARITY_CI.md`.
