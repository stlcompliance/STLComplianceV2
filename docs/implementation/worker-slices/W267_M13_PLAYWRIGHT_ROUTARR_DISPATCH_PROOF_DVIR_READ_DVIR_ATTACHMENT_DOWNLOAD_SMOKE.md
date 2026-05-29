# W267 — M13 Playwright: RoutArr dispatch proof/DVIR read DVIR attachment download smoke

Builds on **W261** (trip capture attachments API + `TripProofDvirReadPanel` `dvir-attachment-*` download buttons), **W265** (dispatch proof attachment download smoke pattern), and **W217** (proof/DVIR persistence).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-proof-dvir-read-dvir-attachment-download-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `dvir-attachment-{id}` link visible with `photo:` label → click triggers browser download with expected filename |

Fixture helper `ensureRoutArrDvirAttachmentDownloadFixture` seeds a dispatched trip with pre-trip DVIR pass and a JPEG photo attachment uploaded via API (dispatcher read-only UI path; no driver-portal interaction in the browser).

No proof attachment download, proof/DVIR capture, or trip start/complete in this smoke.

### Frontend tests

- `TripProofDvirReadPanel.test.tsx` — dispatcher clicks `dvir-attachment-*` invokes `downloadTripCaptureAttachment` with `dvir` subject type

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchProofDvirReadDvirAttachmentDownloadSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w267`
- `All.Count >= 35`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-proof-dvir-read-dvir-attachment-download-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Panel renders when `canAssignDrivers` is true (dispatcher/admin handoff).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd apps/routarr-frontend
npm test -- --run TripProofDvirReadPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-dispatch-proof-dvir-read-dvir-attachment-download-smoke.spec.ts
```

## Out of scope

- Proof attachment download on read panel (`proof-attachment-*`; W265)
- Driver-portal DVIR/document upload in browser (W259/W266)
- Document attachment kind on dispatch read path
- Attachment retention worker

## Next slice

- ~~**M13 Playwright** — RoutArr dispatch proof/DVIR read document attachment download smoke (`document:` on proof or DVIR row; builds on W266/W267)~~ → **W268 complete**
- ~~**M13 Playwright** — RoutArr end-to-end document journey smoke (driver upload + dispatcher read download in one session; builds on W266/W268)~~ → **W269 complete**
- **M13 Playwright** — RoutArr end-to-end photo attachment journey smoke (driver-portal pickup photo upload → dispatch read panel photo download in one browser session; builds on W264/W265)
- **M13** — Attachment retention worker or end-to-end journey combining driver document upload + dispatcher read download
