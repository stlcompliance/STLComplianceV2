# W242 — M13 Playwright: Compliance Core audit delivery orchestration smoke

Builds on **W232** (suite handoff product admin Playwright pattern) and **W240** (Audit delivery orchestration Admin panel).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `compliancecore-audit-delivery-orchestration-smoke.spec.ts` | `/admin` | Suite sign-in → handoff → `compliancecore-audit-delivery-orchestration-panel`: scheduled evaluation, M12 batch, and audit package job sections load with status text; manual trigger buttons visible/enabled for scheduled eval (M12 trigger visible; may be disabled when worker off) |

Trigger clicks are **out of scope** — avoids destructive batch/eval side effects in shared demo tenant.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreAuditDeliveryOrchestrationSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w242`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-audit-delivery-orchestration-smoke.spec.ts
```

Requires Compliance Core API (5107) and frontend (5177). Demo platform admin has orchestration read + trigger permissions.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Clicking **Run scheduled evaluation now** or **Run M12 batch now**
- M12 worker settings save flow (W232 `compliancecore-m12-worker-settings-smoke.spec.ts`)

## Next slice

- **RoutArr** — dispatch exception queue Playwright (W210) → **W243 complete**
- **SupplyArr M8/M10** — procurement automation / coordination UX depth
- **Suite M13** — NexArr platform-admin orchestration parity (if new panels ship)
