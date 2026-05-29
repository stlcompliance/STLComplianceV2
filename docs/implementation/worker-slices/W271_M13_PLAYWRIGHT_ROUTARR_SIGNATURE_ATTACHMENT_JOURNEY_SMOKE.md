# W271 — M13 Playwright: RoutArr end-to-end signature attachment journey smoke

Builds on **W264** (driver-portal delivery signature pad upload + `ensureRoutArrAttachmentUploadFixture`), **W265** (dispatch proof/DVIR read attachment download smoke), **W270** (end-to-end photo attachment journey pattern), **W261** (trip capture attachments API + `TripProofDvirReadPanel` download buttons), and **W217** (proof/DVIR persistence).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-signature-attachment-journey-smoke.spec.ts` | `/driver-portal` → `/dispatch` | Suite sign-in → handoff → driver portal **Start trip** → **Quick delivery proof** → delivery **Signature** pad draw + save → `signature:` row visible → navigate to `trip-proof-dvir-read-panel` → fixture trip ID → **Load execution** → `proof-attachment-*` link with matching `signature: signature.png` → click triggers browser download |

Fixture helper `ensureRoutArrSignatureAttachmentUploadFixture` seeds a dispatched trip with pickup proof but **no** pre-uploaded signature — the browser starts the trip, creates delivery proof, and saves the signature; the dispatcher read panel verifies persistence and download in the same session.

No pickup photo/document upload, DVIR capture, trip complete, or API-seeded attachments in this smoke.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSignatureAttachmentJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w271`
- `All.Count >= 39`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-signature-attachment-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin handoff must reach both driver portal and dispatch read panel (`canAssignDrivers` for dispatcher path).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-signature-attachment-journey-smoke.spec.ts
```

## Out of scope

- Pickup photo end-to-end journey (W270)
- DVIR signature/photo end-to-end journey
- Trip complete after signature upload in the same session
- Attachment retention worker
- API-seeded attachment download-only path (W265)

## Next slice

- **M13** — Attachment retention worker or broader proof/DVIR E2E journey (trip start after document upload + dispatcher verification)
- **M13 Playwright** — RoutArr end-to-end DVIR photo attachment journey smoke (driver-portal pre-trip DVIR photo upload → dispatch read panel DVIR attachment download in one browser session; builds on W267/W270/W271)
