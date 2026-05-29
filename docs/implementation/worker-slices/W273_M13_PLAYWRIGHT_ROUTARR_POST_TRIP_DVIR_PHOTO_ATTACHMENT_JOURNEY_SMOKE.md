# W273 — M13 Playwright: RoutArr end-to-end post-trip DVIR photo attachment journey smoke

Builds on **W272** (pre-trip DVIR photo end-to-end journey + `ensureRoutArrDvirPhotoAttachmentUploadFixture`), **W267** (dispatch proof/DVIR read DVIR attachment download smoke), **W261** (trip capture attachments API + `TripProofDvirReadPanel` `dvir-attachment-*` download buttons), and **W217** (proof/DVIR persistence).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-post-trip-dvir-photo-attachment-journey-smoke.spec.ts` | `/driver-portal` → `/dispatch` | Suite sign-in → handoff → driver portal **Start trip** → **Submit post-trip DVIR** → `capture-attachments-dvir-*` photo file upload → `photo:` row visible → navigate to `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `dvir-attachment-*` link with matching `photo: posttrip-dvir-photo-e2e-*.jpg` → click triggers browser download |

Fixture helper `ensureRoutArrPostTripDvirPhotoAttachmentUploadFixture` seeds a dispatched trip with relaxed pre-trip gates but **no** post-submitted DVIR or photo — the browser starts the trip, submits post-trip DVIR, uploads the photo, and the dispatcher read panel verifies persistence and download in the same session.

No pre-trip DVIR capture, trip complete/close, or API-seeded DVIR attachments in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrPostTripDvirPhotoAttachmentJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w273`
- `All.Count >= 40`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-post-trip-dvir-photo-attachment-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must reach both driver portal and dispatch read panel (`canAssignDrivers` for dispatcher path).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-post-trip-dvir-photo-attachment-journey-smoke.spec.ts
```

## Out of scope

- Pre-trip DVIR photo end-to-end journey (W272)
- Trip complete/close after post-trip DVIR photo in the same session
- Attachment retention worker
- API-seeded DVIR attachment download-only path (W267)

## Next slice

- **M13** — Attachment retention worker or broader proof/DVIR E2E journey (trip complete after post-trip DVIR photo + dispatcher verification)
- **M13 Playwright** — RoutArr end-to-end trip-complete-after-capture journey smoke (driver-portal complete trip after post-trip DVIR photo upload; builds on W273)
