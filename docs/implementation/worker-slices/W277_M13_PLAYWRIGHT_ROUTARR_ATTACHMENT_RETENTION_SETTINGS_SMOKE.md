# W277 — M13 Playwright: RoutArr settings attachment retention panel smoke

Builds on **W276** (`AttachmentRetentionSettingsPanel`, `/api/attachment-retention-settings`, retention runs API), **W263** (RoutArr `/settings` save/reload Playwright pattern), and **W241** (RoutArr internal batch helper conventions in `e2eApi`).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-attachment-retention-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `attachment-retention-settings-panel`: heading; enable checkbox + retention days input; toggle enable + change days → **Save retention settings** → reload verifies persistence → **Recent retention runs** section shows empty state or run list → restore original enable/days |

Save/restore keeps shared demo tenant stable. No live attachment retention purge batch in this smoke (read-only worker path; avoids deleting journey fixture attachments).

### `e2eApi` helpers (optional for future fixture smokes)

- `upsertRoutArrAttachmentRetentionSettings`
- `processRoutArrAttachmentRetentionBatch`
- `issueRoutArrAttachmentRetentionWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsAttachmentRetentionSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w277`
- `All.Count >= 42`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-attachment-retention-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo dispatcher/admin role can manage notification/retention settings (`canManageNotificationSettings`).

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-attachment-retention-smoke.spec.ts
```

## Out of scope

- Live shared-worker attachment purge batch (would delete demo capture attachments)
- Pre-purge export snapshot (W276 gap)
- Trip execution or notification settings panels on same `/settings` page

## Next recommended slice

**M13 Playwright — RoutArr settings trip completion rollup panel smoke** (handoff → `/settings` `trip-completion-rollup-settings-panel`, enable toggle + interval save/reload, recent runs section; builds on W176/W263/W277).
