# W313 — M13 Playwright: SupplyArr procurement exception investigate → resolve → close journey (W303/W312 follow-up)

Builds on **W250** (procurement exception resolution depth + templates), **W303** (investigate → resolve journey), **W311/W312** (link PR/PO → resolve journeys).

Completes the resolve path lifecycle in one browser flow: open fixture → **Investigate** → **Resolve** with PR resubmit template → **Close** with API asserts for resolved template, closed status, and `closedAt`.

## Scope

### Frontend (`ProcurementExceptionsPanel`)

Reuses existing test ids from W303/W304:

| Test id | Element |
|---------|---------|
| `procurement-exception-resolution-template` | Resolution template select |
| `procurement-exception-status-{exceptionId}` | Status badge |
| `procurement-exception-investigate-{exceptionId}` | Investigate action |
| `procurement-exception-resolve-{exceptionId}` | Resolve action |
| `procurement-exception-close-{exceptionId}` | Close action (visible when resolved) |

No panel code changes required.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-close-after-resolve-journey-smoke.spec.ts` | `/purchasing` | Template → investigate → resolve → close with API asserts |

No waive/cancel/link mutations. Uses minimal PR + open exception fixture (same category as W303).

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionCloseAfterResolveJourneyFixture`
- `assertSupplyArrProcurementExceptionClosedAfterResolveFromHandoff`

Reuses W303 `createMinimalPurchaseRequestForExceptions` + `createProcurementExceptionForPurchaseRequest` internals and existing status/resolved assert helpers.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionCloseAfterResolveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w313`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-close-after-resolve-journey-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin with procurement exception manage permission.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- --run ProcurementExceptionsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-close-after-resolve-journey-smoke.spec.ts
```

## Out of scope

- Post-cancel reopen (no API)
- Link PR/PO resolve paths (covered by W311/W312)
- Waive/close path (covered by W304)
- Escalation or notification process-batch journeys (W301/W302)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception close-after-link-PR-resolve journey smoke (W311 + close), RoutArr product-admin smokes, or post-cancel reopen only if API gains reopen support
