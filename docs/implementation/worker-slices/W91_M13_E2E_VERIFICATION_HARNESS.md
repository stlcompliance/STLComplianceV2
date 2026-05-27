# Worker 91 — M13 E2E verification harness

## Slice name

M13 load/E2E verification harness — cross-product integration journeys, optional live-stack smoke tests, CI-friendly skip behavior.

## Products touched

- **tests/STLCompliance.E2E** — new test project (integration + live modes)
- **STLCompliance.slnx** — project registration
- **docs/implementation/worker-slices/00_SLICE_STATE.md** — Worker 91 completion

## Harness design

### Integration mode (default, CI)

Uses `WebApplicationFactory` with in-memory EF databases and HTTP handler wiring between NexArr and product APIs — same proven pattern as product `*.Auth.Tests` cross-product suites, but orchestrated as **user-visible journeys** rather than single-feature assertions.

Traits: `[Trait("Category", "Integration")]`

### Live mode (optional)

Probes docker-compose host URLs (`5101`–`5107`) via `/health` and demo NexArr login. Uses `Xunit.SkippableFact` to skip when:

- `E2E_LIVE` is not `1`/`true`, or
- services are unreachable

Traits: `[Trait("Category", "Live")]`

## Cross-product flows (5)

| Flow | Journey |
|------|---------|
| NexArr handoff | Login → `/api/me` → handoff code → StaffArr/RoutArr redeem |
| StaffArr readiness | Person without certs `not_ready` → grant baseline certs → `ready` |
| TrainArr assignment complete | Incident route → assignment → evaluations/signoffs → complete → StaffArr cert + training blocker cleared |
| MaintainArr work order | Handoff → asset seed → WO open → in_progress → completed |
| RoutArr dispatch assign | Trip → Compliance Core workflow gate block → preview conflicts → override assign |

## Verification commands

```powershell
dotnet build "STLCompliance.slnx" -c Release
dotnet test "tests/STLCompliance.E2E/STLCompliance.E2E.csproj" -c Release --filter "Category=Integration"

# Optional live stack
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
$env:E2E_LIVE = "1"
dotnet test "tests/STLCompliance.E2E/STLCompliance.E2E.csproj" -c Release --filter "Category=Live"
```

## Gap analysis vs M13 milestone plan

After reviewing `01_MILESTONE_MASTERPLAN.md`, `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`, and `feature_to_milestone_matrix.csv`, the following **required features remain clearly incomplete** (not implemented in this slice):

### M13 ship-gate items still open

| Area | Gap |
|------|-----|
| **Load / performance** | No k6, NBomber, or dedicated load-test harness; no SLO baseline runs |
| **Browser E2E** | Suite Frontend backlog lists Playwright E2E coverage (M3/M13); only API-level integration E2E exists |
| **OpenAPI parity** | Matrix requires APIs documented in OpenAPI; no automated OpenAPI ↔ implementation drift check in CI |
| **Observability** | No centralized metrics/tracing/dashboards validation in acceptance suite |
| **Recovery / DR** | No automated backup-restore or failover verification |
| **Tenant isolation soak** | Cross-product tests cover happy/deny paths per slice; no dedicated multi-tenant isolation E2E battery |

### Other milestone gaps (outside M13 scope but ship-relevant)

| Product / area | Gap |
|----------------|-----|
| **STLComplianceSite** | Public marketing site (homepage, product pages, SEO, demo path) — M3/M12 backlog, no app evidence |
| **NexArr audit export** | M12 `audit export` not evidenced |
| **StaffArr audit packages** | M12 `/api/audit-packages` not evidenced |
| **TrainArr workers** | Several M12 workers listed in masterplan (recertification assignment, publish retry, notification dispatch, evidence retention, orphan detection) — partial coverage via shared-worker slices only |
| **Companion app** | Offline queue, push notifications, field evidence capture — deferred in W90 |
| **Shared UI package** | W88 noted `packages/ui` extraction not done |

### Completed in this slice

- M13 E2E verification harness (API integration journeys + optional live health/login smoke)
- CI-safe default (`Category=Integration` always runs; `Category=Live` opt-in)

## Remaining blocked items

| Item | Blocker |
|------|---------|
| Playwright browser journeys | Requires stable frontend base URLs + seed data in docker-compose; not blocking API integration E2E |
| Load testing | Needs performance targets / SLO definitions from product owners |
| OpenAPI drift gate | Needs published OpenAPI artifacts per API (may exist but not wired to CI) |

## Next slice

Per `00_SLICE_STATE.md` — M13 remaining hardening (load tests, OpenAPI parity CI, Playwright browser E2E, observability checks) or ship-gate acceptance sweep.
