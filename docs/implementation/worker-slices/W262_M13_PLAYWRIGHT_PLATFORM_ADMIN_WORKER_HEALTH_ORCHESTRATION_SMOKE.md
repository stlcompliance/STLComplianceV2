# W262 — M13 Playwright: NexArr platform-admin worker health orchestration smoke

Builds on **W260** (platform-admin worker health orchestration UI) and **W138/W242** (suite platform-admin Playwright patterns).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `platform-admin-worker-health-orchestration-smoke.spec.ts` | `/app/platform-admin/orchestration` | Suite sign-in → `platform-worker-health-orchestration-panel`: product health badge + probe row, service token inventory counts, three lifecycle worker cards (cleanup / entitlement / tenant lifecycle) with pending or run history text; manual trigger buttons visible |

Trigger clicks are **out of scope** — avoids service token purge, entitlement reconcile, and tenant lifecycle side effects in shared demo tenant (same read-only posture as W242).

No `e2eApi` helpers required (GET-only UI smoke).

### Catalog

- `StlE2ePlaywrightSpecCatalog.PlatformAdminWorkerHealthOrchestrationSmokeSpec` in `PlatformAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Platform_admin_smoke_specs_include_audit_export_and_worker_health`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/platform-admin-worker-health-orchestration-smoke.spec.ts
```

Requires suite frontend (5174) and NexArr API (5101). Demo platform admin (`admin@demo.stl`) has orchestration read permissions.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
```

## Out of scope

- Clicking **Run cleanup now**, **Run reconciliation now**, or **Run lifecycle now**
- Enabling disabled workers via settings pages
- Product handoff journey (panel lives in suite platform-admin shell)

## Next slice

- ~~**M13 Playwright** — RoutArr trip execution settings panel smoke (optional `/settings` companion to W259)~~ → **W263 complete**
- **M13 Playwright** — RoutArr driver-portal attachment upload smoke (photo/signature path; builds on W261)
