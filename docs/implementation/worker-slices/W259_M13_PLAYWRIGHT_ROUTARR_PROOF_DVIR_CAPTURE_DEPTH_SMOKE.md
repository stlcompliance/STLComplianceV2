# W259 — M13 Playwright: RoutArr proof/DVIR capture depth smoke (W257)

Builds on **W257** (`TripExecutionCaptureRules`, capture-readiness API, driver-portal start gates, `DriverPortalPanel` capture UX), **W247** (driver portal Playwright handoff pattern), and **W248/W256/W258** (read-only mutation pattern for rejected validation paths).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-driver-portal-proof-dvir-capture-depth-smoke.spec.ts` | `/driver-portal` | Suite sign-in → handoff → dispatched trip card; `driver-portal-proof-dvir-{tripId}` section; **start blocked** / pre DVIR hint; **Start trip** disabled; `capture-readiness-blockers` when policy unsatisfied; pre-trip `dvir-form-pre_trip` + quick proof controls visible; select **Fail** → defect notes textarea → submit without notes → `role="alert"` error (rejected mutation only) |

Successful pre-trip DVIR submit, proof capture, dispatch/start/complete clicks are **out of scope** — matches W253/W256/W258 read-only pattern.

### e2eApi fixture

`ensureRoutArrProofDvirCaptureFixture()` in `support/e2eApi.ts`:

- `PUT /api/trip-execution-settings` — require pre-trip DVIR before start
- `POST /api/trips` — dispatched-ready trip with vehicle ref
- `PATCH assign-driver` → demo admin / journey subject person
- `PATCH status` → `dispatched`
- Returns `{ tripId }`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDriverPortalProofDvirCaptureDepthSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w259`
- `All.Count >= 30`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-driver-portal-proof-dvir-capture-depth-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo platform admin is journey subject (`22222222-2222-2222-2222-222222222201`) and sees assigned dispatched trips in driver portal.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build packages/shared-dotnet/STLCompliance.Shared/STLCompliance.Shared.csproj -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-driver-portal-proof-dvir-capture-depth-smoke.spec.ts
```

## Out of scope

- Pass/conditional DVIR submit or proof capture that persists records
- Start/complete/dispatch driver-portal lifecycle clicks
- `/settings` `TripExecutionSettingsPanel` save (W257 API covered by integration tests)
- Photo/document/signature attachments (deferred from W257)
- Dispatch read panel (`trip-proof-dvir-read-panel`; see W248)

## Next slice

- ~~**M13 Playwright** — RoutArr trip execution settings panel smoke (optional `/settings` companion to W259)~~ → **W263 complete**
- **M13 Playwright** — RoutArr driver-portal attachment upload smoke (photo/signature path; builds on W261)
