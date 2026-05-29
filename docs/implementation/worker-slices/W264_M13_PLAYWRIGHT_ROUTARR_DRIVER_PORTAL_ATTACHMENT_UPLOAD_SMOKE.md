# W264 — M13 Playwright: RoutArr driver-portal attachment upload smoke

Builds on **W261** (trip capture attachments API + `TripCaptureAttachmentPanel`), **W263** (trip execution settings panel smoke), **W259** (driver-portal capture depth companion), and **W147** (companion photo upload Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-driver-portal-attachment-upload-smoke.spec.ts` | `/driver-portal` | Suite sign-in → handoff → fixture trip card → pickup proof attachment panel: JPEG photo upload via file input → readiness gate clears → **Start trip** → quick delivery proof → signature pad draw + **Save signature** → attachment rows list `photo:` and `signature:` |

Fixture helper `ensureRoutArrAttachmentUploadFixture` seeds tenant policy (pickup photo + delivery signature required), creates a today-scheduled dispatched trip assigned to the demo driver, and pre-creates pickup proof only (uploads happen in the browser).

No dispatcher download or trip complete mutations in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDriverPortalAttachmentUploadSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w264`
- `All.Count >= 32`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-driver-portal-attachment-upload-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must map to the assigned driver person (`journeySubjectPersonId`).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-driver-portal-attachment-upload-smoke.spec.ts
```

## Out of scope

- ~~Document attachment upload (separate optional smoke)~~ → **W266 complete**
- ~~Dispatcher attachment download on `/dispatch` read panel (W248 follow-up)~~ → **W265 complete**
- Trip complete/close after signature
- Attachment retention worker

## Next slice

- ~~**M13 Playwright** — RoutArr dispatch proof/DVIR read attachment download smoke~~ → **W265 complete**
