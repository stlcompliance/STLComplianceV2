# M13 OTEL smoke checks and metrics wiring

## Slice name

M13 OTEL smoke checks — wire OpenTelemetry in shared API/worker hosts, platform health metrics, observability probe endpoint, automated smoke tests, and ops script.

## Products touched

- **STLCompliance.Shared** — `StlOpenTelemetryOptions`, `StlOpenTelemetryExtensions`, `StlPlatformMetrics`, `StlObservabilityStatus`
- **All 7 product APIs** — via `StlApiHost` (`/health/observability`, health request metrics when OTEL enabled)
- **shared-worker + product workers** — via `StlWorkerHost`
- **tests/STLCompliance.Otel.Tests** — configuration parsing, API smoke checks (×7), worker status test
- **scripts/ops/otel-smoke.ps1** — operator probe against running APIs
- **CI** — `Category=Otel` step in `.github/workflows/ci.yml`

## Shared additions

| File | Purpose |
|------|---------|
| `Observability/StlOpenTelemetryOptions.cs` | Parse `OTEL_ENABLED`, `OTEL_SERVICE_NAME`, `OTEL_EXPORTER_OTLP_ENDPOINT` |
| `Observability/StlOpenTelemetryExtensions.cs` | Register tracing/metrics exporters (OTLP or console in dev/test) |
| `Observability/StlPlatformMetrics.cs` | `STLCompliance.Platform` meter; `stl.health.requests` counter |
| `Observability/StlObservabilityStatus.cs` | JSON shape for `/health/observability` |

When `OTEL_ENABLED=false` (Render default), hosts behave as before — no OTEL providers registered.

## API surface

| Method | Path | Auth | Notes |
|--------|------|------|-------|
| GET | `/health/observability` | Anonymous | Reports OTEL enabled flag, service name, exporter mode, configured meters/sources |

## Verification commands

```powershell
dotnet build "STLCompliance.slnx" -c Release
dotnet test "STLCompliance.slnx" -c Release --filter "Category!=Live"
dotnet test "tests/STLCompliance.Otel.Tests/STLCompliance.Otel.Tests.csproj" -c Release --filter "Category=Otel"
```

Local operator smoke (APIs running on default ports):

```powershell
./scripts/ops/otel-smoke.ps1
./scripts/ops/otel-smoke.ps1 -RequireOtelEnabled  # after setting OTEL_ENABLED=true
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| OTEL / metrics dashboards | **Wired** — instrumentation + smoke checks; connect Render `OTEL_EXPORTER_OTLP_ENDPOINT` when backend available |
| Load / performance | Still blocked — needs SLO definitions |
| DR / backup restore | Still open |

## Next slice

DR restore drill script for managed Postgres backup verification; load-test harness once product owners publish SLO targets.
