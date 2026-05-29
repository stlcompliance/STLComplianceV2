# W266 — M13 Playwright: RoutArr driver-portal document attachment upload smoke

Builds on **W261** (trip capture attachments API + `TripCaptureAttachmentPanel` Document control), **W264** (driver-portal photo/signature upload companion), and **W147** (companion file upload Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-driver-portal-document-attachment-upload-smoke.spec.ts` | `/driver-portal` | Suite sign-in → handoff → fixture trip card → pickup proof attachment panel: **Document** file input uploads minimal PDF → `document:` row with filename → **Start trip** enabled (no photo/signature gates) |

Fixture helper `ensureRoutArrDocumentAttachmentUploadFixture` seeds tenant policy (pickup proof required; photo/signature/DVIR gates off), creates a today-scheduled dispatched trip assigned to the demo driver, and pre-creates pickup proof only (document upload happens in the browser).

No trip start click, delivery proof, signature pad, or dispatcher download in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDriverPortalDocumentAttachmentUploadSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w266`
- `All.Count >= 34`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-driver-portal-document-attachment-upload-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must map to the assigned driver person (`journeySubjectPersonId`).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-driver-portal-document-attachment-upload-smoke.spec.ts
```

## Out of scope

- Photo/signature upload paths (W264)
- Dispatcher attachment download on `/dispatch` read panel (W265)
- Trip start/complete after document upload
- Document-required tenant policy gate (no API field yet)
- Attachment retention worker

## Next slice

- ~~**M13 Playwright** — RoutArr dispatch proof/DVIR read DVIR attachment download smoke (`dvir-attachment-*`; builds on W265)~~ → **W267 complete**
- ~~**M13 Playwright** — RoutArr dispatch proof/DVIR read document attachment download smoke (dispatcher download path; builds on W266)~~ → **W268 complete**
