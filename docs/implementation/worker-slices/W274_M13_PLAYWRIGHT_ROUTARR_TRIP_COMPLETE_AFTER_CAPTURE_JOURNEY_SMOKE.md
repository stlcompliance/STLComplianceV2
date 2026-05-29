# W274 — M13 Playwright: RoutArr end-to-end trip-complete-after-capture journey smoke

Builds on **W273** (post-trip DVIR photo end-to-end journey + `ensureRoutArrPostTripDvirPhotoAttachmentUploadFixture`), **W267** (dispatch proof/DVIR read DVIR attachment download smoke), **W257** (trip execution capture gates + `requirePostTripDvirPhotoBeforeComplete`), and **W213** (driver portal complete action).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-trip-complete-after-capture-journey-smoke.spec.ts` | `/driver-portal` → `/dispatch` | Suite sign-in → handoff → driver portal **Start trip** → **Submit post-trip DVIR** → `capture-attachments-dvir-*` photo upload → **Complete disabled until photo** → **Complete** → trip card shows **completed** → navigate to `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `post DVIR yes` + `dvir-attachment-*` download |

Fixture helper `ensureRoutArrTripCompleteAfterCaptureFixture` seeds a dispatched trip with `requirePostTripDvirPhotoBeforeComplete: true` and relaxed pre-trip gates — the browser performs capture, completes the trip, and the dispatcher read panel verifies persisted DVIR + attachment download in the same session.

No API-seeded DVIR attachments, trip close, or dispatch closeout in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrTripCompleteAfterCaptureJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w274`
- `All.Count >= 40`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-trip-complete-after-capture-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must reach both driver portal and dispatch read panel (`canAssignDrivers` for dispatcher path).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-trip-complete-after-capture-journey-smoke.spec.ts
```

## Out of scope

- Post-trip DVIR photo download-only journey without complete (W273)
- Trip close after complete
- Dispatch closeout apply
- Attachment retention worker

## Next slice

- **M12** — Attachment retention worker (`trainarr.evidence.retention.purge` pattern for RoutArr trip capture attachments)
- **M13 Playwright** — RoutArr end-to-end trip-close-after-complete journey smoke (driver-portal Close after complete + dispatcher verification; builds on W274)
