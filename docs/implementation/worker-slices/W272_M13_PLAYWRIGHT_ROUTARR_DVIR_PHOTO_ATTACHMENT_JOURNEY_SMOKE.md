# W272 — M13 Playwright: RoutArr end-to-end DVIR photo attachment journey smoke

Builds on **W267** (dispatch proof/DVIR read DVIR attachment download smoke + `ensureRoutArrDvirAttachmentDownloadFixture`), **W270** (end-to-end photo attachment journey pattern), **W271** (end-to-end signature attachment journey pattern), **W261** (trip capture attachments API + `TripProofDvirReadPanel` `dvir-attachment-*` download buttons), and **W217** (proof/DVIR persistence).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dvir-photo-attachment-journey-smoke.spec.ts` | `/driver-portal` → `/dispatch` | Suite sign-in → handoff → driver portal **Submit pre-trip DVIR** → `capture-attachments-dvir-*` photo file upload → `photo:` row visible → navigate to `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `dvir-attachment-*` link with matching `photo: pretrip-dvir-photo-e2e-*.jpg` → click triggers browser download |

Fixture helper `ensureRoutArrDvirPhotoAttachmentUploadFixture` seeds a dispatched trip with pre-trip DVIR policy but **no** pre-submitted DVIR or photo — the browser submits DVIR, uploads the photo, and the dispatcher read panel verifies persistence and download in the same session.

No pickup/delivery proof capture, trip start/complete, or API-seeded DVIR attachments in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDvirPhotoAttachmentJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w272`
- `All.Count >= 40`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dvir-photo-attachment-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must reach both driver portal and dispatch read panel (`canAssignDrivers` for dispatcher path).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-dvir-photo-attachment-journey-smoke.spec.ts
```

## Out of scope

- Proof pickup/delivery photo/signature end-to-end journeys (W270/W271)
- Post-trip DVIR photo end-to-end journey
- Trip start after DVIR photo upload in the same session
- Attachment retention worker
- API-seeded DVIR attachment download-only path (W267)

## Next slice

- **M13** — Attachment retention worker or broader proof/DVIR E2E journey (trip start after document upload + dispatcher verification)
- **M13 Playwright** — RoutArr end-to-end post-trip DVIR photo attachment journey smoke (driver-portal post-trip DVIR photo upload → dispatch read panel DVIR attachment download in one browser session; builds on W272/W267)
