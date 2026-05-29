# W269 — M13 Playwright: RoutArr end-to-end document attachment journey smoke

Builds on **W266** (driver-portal document upload smoke + `ensureRoutArrDocumentAttachmentUploadFixture`), **W268** (dispatch proof/DVIR read document attachment download smoke), **W261** (trip capture attachments API + `TripProofDvirReadPanel` download buttons), and **W217** (proof/DVIR persistence).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-document-attachment-journey-smoke.spec.ts` | `/driver-portal` → `/dispatch` | Suite sign-in → handoff → driver portal pickup BOL **Document** file upload → `document:` row visible → navigate to `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `proof-attachment-*` link with matching `document:` filename → click triggers browser download |

Fixture helper `ensureRoutArrDocumentAttachmentUploadFixture` seeds a dispatched trip with pickup proof but **no** pre-uploaded attachment — the browser performs the document upload; the dispatcher read panel verifies persistence and download in the same session.

No photo/signature upload, DVIR capture, trip start/complete, or API-seeded attachments in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDocumentAttachmentJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w269`
- `All.Count >= 37`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-document-attachment-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must reach both driver portal and dispatch read panel (`canAssignDrivers` for dispatcher path).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-document-attachment-journey-smoke.spec.ts
```

## Out of scope

- Photo/signature end-to-end journey (W264/W265 pattern)
- DVIR document end-to-end journey
- Trip start after document upload in the same session
- Attachment retention worker
- API-seeded attachment download-only path (W268)

## Next slice

- **M13 Playwright** — RoutArr end-to-end photo attachment journey smoke (driver-portal pickup photo upload → dispatch read panel photo download in one browser session; builds on W264/W265)
- **M13** — Attachment retention worker or broader proof/DVIR E2E journey (trip start after document upload + dispatcher verification)
