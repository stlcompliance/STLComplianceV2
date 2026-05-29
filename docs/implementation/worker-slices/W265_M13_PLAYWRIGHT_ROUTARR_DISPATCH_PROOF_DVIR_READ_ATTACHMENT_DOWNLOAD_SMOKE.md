# W265 — M13 Playwright: RoutArr dispatch proof/DVIR read attachment download smoke

Builds on **W261** (trip capture attachments API + `TripProofDvirReadPanel` download buttons), **W248** (dispatch proof/DVIR read panel smoke), and **W264** (driver-portal attachment upload companion).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-proof-dvir-read-attachment-download-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `proof-attachment-{id}` link visible with `photo:` label → click triggers browser download with expected filename |

Fixture helper `ensureRoutArrAttachmentDownloadFixture` seeds a dispatched trip with pickup proof and a JPEG photo attachment uploaded via API (dispatcher read-only UI path; no driver-portal interaction in the browser).

No proof/DVIR capture, trip start/complete, or DVIR attachment download in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchProofDvirReadAttachmentDownloadSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w265`
- `All.Count >= 33`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-proof-dvir-read-attachment-download-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Panel renders when `canAssignDrivers` is true (dispatcher/admin handoff).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-dispatch-proof-dvir-read-attachment-download-smoke.spec.ts
```

## Out of scope

- Driver-portal attachment upload in browser (W264)
- DVIR attachment download on read panel (`dvir-attachment-*`)
- ~~Document attachment kind on dispatch read path~~ → **W268 complete**
- Attachment retention worker

## Next slice

- ~~**M13 Playwright** — RoutArr driver-portal document attachment upload smoke (optional companion to W264)~~ → **W266 complete**
- ~~**M13 Playwright** — RoutArr dispatch proof/DVIR read DVIR attachment download smoke (`dvir-attachment-*`; builds on W265)~~ → **W267 complete**
