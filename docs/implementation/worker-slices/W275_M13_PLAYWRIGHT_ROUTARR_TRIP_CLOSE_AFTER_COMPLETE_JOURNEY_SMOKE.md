# W275 — M13 Playwright: RoutArr end-to-end trip-close-after-complete journey smoke

Builds on **W274** (trip-complete-after-capture journey + `ensureRoutArrTripCompleteAfterCaptureFixture`), **W251** (dispatch closeout depth), and **W253** (dispatch closeout panel smoke).

## Scope

### API / domain (`routarr-api`)

| Change | Purpose |
|--------|---------|
| `Trip.ClosedAt` + migration `RoutArrTripDriverClose` | Driver-portal Close after Complete records driver acknowledgment |
| `TripService.AcknowledgeDriverCloseAsync` | Sets `ClosedAt` when trip is already `completed` |
| `DriverPortalService` schedule + `CanClose` | Completed trips stay on Today until Close; Close removes them from schedule |
| `TripExecutionSummaryResponse` | Adds `dispatchStatus` + `closedAt` for dispatcher read verification |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-trip-close-after-complete-journey-smoke.spec.ts` | `/driver-portal` → `/dispatch` | Suite sign-in → handoff → **Start trip** → post-trip DVIR + photo → **Complete** → **Close** (card leaves schedule) → `trip-proof-dvir-read-panel` → `status completed` + `driver closed yes` + post DVIR + attachment download |

Fixture helper `ensureRoutArrTripCloseAfterCompleteFixture` reuses W274 trip seed (`requirePostTripDvirPhotoBeforeComplete: true`).

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrTripCloseAfterCompleteJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w275`
- `All.Count >= 41`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-trip-close-after-complete-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must reach driver portal and dispatch read panel.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~Driver_portal"
dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-trip-close-after-complete-journey-smoke.spec.ts
```

## Out of scope

- Dispatch closeout apply (W251/W253)
- Attachment retention worker
- Trip close without prior Complete (legacy in-progress Close path unchanged)

## Next slice

- **M12** — Attachment retention worker for RoutArr trip capture attachments (`routarr.capture.attachments.retention` pattern)
