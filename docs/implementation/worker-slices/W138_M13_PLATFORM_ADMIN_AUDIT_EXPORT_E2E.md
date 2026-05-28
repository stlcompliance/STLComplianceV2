# Worker 138 — M13 Playwright platform-admin audit export smoke

## Scope

Extends the Playwright E2E harness (W94/W134) with **suite platform-admin audit export** coverage:

- Login as demo platform admin (`admin@demo.stl`)
- Navigate `/app/platform-admin/audit-export`
- Assert manifest sections and timeline preview load
- **Sync export** — Preview JSON + Download ZIP (Playwright download event)
- **Background job** — UI creates job; `e2eApi` issues `shared-worker` service token and calls `POST /api/internal/platform-audit-package-jobs/process-batch` (no shared-worker container required for smoke)
- **Catalog** — `StlE2ePlaywrightSpecCatalog.PlatformAdminAuditExportSmokeSpec`
- **UI test ids** — `platform-audit-export-panel`, manifest/timeline/job/json preview

## Spec (skip unless `E2E_LIVE=1`)

| File | Coverage |
|------|----------|
| `platform-admin-audit-export-smoke.spec.ts` | Platform-admin audit package manifest, timeline, sync ZIP/JSON, background ZIP job completion |

## Verification

```powershell
./scripts/ops/e2e-stack-up.ps1
./scripts/ops/e2e-frontends-preview.ps1  # suite on 5174
$env:E2E_LIVE = "1"
cd tests/e2e-playwright
npm ci
npx playwright install chromium
npm test
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=E2e"
```

Without `E2E_LIVE`: spec skipped (exit 0).

## Out of scope

- Full shared-worker container in compose E2E profile (batch triggered via internal API from test helper)
- Tenant-admin forbidden path browser smoke (covered by NexArr API tests)
