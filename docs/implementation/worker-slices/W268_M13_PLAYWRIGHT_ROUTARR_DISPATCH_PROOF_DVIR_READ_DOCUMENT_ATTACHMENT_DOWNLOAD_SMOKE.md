# W268 — M13 Playwright: RoutArr dispatch proof/DVIR read document attachment download smoke

Builds on **W261** (trip capture attachments API + `TripProofDvirReadPanel` `proof-attachment-*` download buttons), **W266** (driver-portal document upload companion), **W265** (dispatch proof photo attachment download pattern), and **W217** (proof/DVIR persistence).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-proof-dvir-read-document-attachment-download-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `proof-attachment-{id}` link visible with `document:` label → click triggers browser download with expected PDF filename |

Fixture helper `ensureRoutArrDocumentAttachmentDownloadFixture` seeds a dispatched trip with pickup proof and a PDF document attachment uploaded via API (dispatcher read-only UI path; no driver-portal interaction in the browser).

No photo/signature attachment download, DVIR document download, proof/DVIR capture, or trip start/complete in this smoke.

### Frontend tests

- `TripProofDvirReadPanel.test.tsx` — dispatcher clicks `proof-attachment-*` invokes `downloadTripCaptureAttachment` with `document` attachment kind on proof row

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchProofDvirReadDocumentAttachmentDownloadSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w268`
- `All.Count >= 36`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-proof-dvir-read-document-attachment-download-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-dispatch-proof-dvir-read-document-attachment-download-smoke.spec.ts
```

## Out of scope

- Photo/signature attachment download on read panel (W265/W267)
- Driver-portal document upload in browser (W266)
- DVIR row document attachment download
- End-to-end journey combining driver document upload + dispatcher read download in one browser session
- Attachment retention worker

## Next slice

- ~~**M13 Playwright** — RoutArr end-to-end document journey smoke (driver-portal document upload → dispatch read panel document download in one session; builds on W266/W268)~~ → **W269 complete**
- **M13 Playwright** — RoutArr end-to-end photo attachment journey smoke (driver-portal pickup photo upload → dispatch read panel photo download in one browser session; builds on W264/W265)
- **M13** — Attachment retention worker or broader proof/DVIR E2E journey (trip start after DVIR document upload on dispatcher read path)
