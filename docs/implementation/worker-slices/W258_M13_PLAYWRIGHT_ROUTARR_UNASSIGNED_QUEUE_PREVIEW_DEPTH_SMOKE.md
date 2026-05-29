# W258 — M13 Playwright: RoutArr unassigned queue preview-before-assign depth smoke (W255)

Builds on **W255** (`UnassignedWorkQueuePanel` urgency filters, preview-before-assign via `confirmDispatchAssignmentPreview`, bulk preview), **W245** (unassigned work queue Playwright smoke), and **W256/W253** (read-only mutation pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-unassigned-queue-preview-depth-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `unassigned-work-queue-panel`; urgent summary header; minutes-until-start on rows; attention filter toggles late vs on-track when fixture seeded; row checkbox selection enables bulk assign + bulk driver select (button unclicked); per-trip driver select + Assign click with dismissed confirm dialog → `unassigned-queue-status` shows **Assignment cancelled.** |

Assign and bulk apply **clicks that would persist driver assignment are out of scope** — preview path only via cancelled confirm (matches W253/W256 read-only pattern).

### e2eApi fixture

`ensureRoutArrUnassignedQueuePreviewFixture()` in `support/e2eApi.ts`:

- Creates one late unassigned trip (`scheduledStartAt` in the past) and one on-track unassigned trip via `POST /api/trips`
- Returns `{ tripIds, lateTripId, onTrackTripId }`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchUnassignedQueuePreviewDepthSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w258`
- `All.Count >= 29`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-unassigned-queue-preview-depth-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo platform admin typically has dispatch assign (`canAssign`).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build packages/shared-dotnet/STLCompliance.Shared/STLCompliance.Shared.csproj -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-dispatch-unassigned-queue-preview-depth-smoke.spec.ts
```

## Out of scope

- Bulk assign / per-trip assign mutations that persist driver assignment
- New unassigned queue API or panel features (see W255)
- Geo map / auto-assign suggestions

## Next slice

- **M13 Playwright** — RoutArr proof/DVIR capture depth smoke (builds on W257) → **W259 complete**
- **NexArr M12** — platform-admin service token / worker health orchestration UI (if scoped)
- **RoutArr M9** — proof photo/document/signature attachments (deferred from W257)
