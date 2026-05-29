# W314 — M13 Playwright: SupplyArr procurement exception investigate → link PR → resolve → close journey (W311/W313 follow-up)

Builds on **W250** (procurement exception resolution depth + templates), **W311** (investigate → link PR → resolve journey), **W313** (close-after-resolve journey).

Completes the link-PR resolve path lifecycle in one browser flow: open fixture → **Investigate** → link follow-up PR → **Save PR/PO links** → **Resolve** with PR resubmit template → **Close** with API asserts for linked PR, resolved template, closed status, and `closedAt`.

## Scope

### Frontend (`ProcurementExceptionsPanel`)

Reuses existing test ids from W303/W310/W311/W313:

| Test id | Element |
|---------|---------|
| `procurement-exception-resolution-template` | Resolution template select |
| `procurement-exception-status-{exceptionId}` | Status badge |
| `procurement-exception-investigate-{exceptionId}` | Investigate action |
| `procurement-exception-resolve-{exceptionId}` | Resolve action |
| `procurement-exception-close-{exceptionId}` | Close action (visible when resolved) |
| `procurement-exception-key-{exceptionId}` | Open detail |
| `procurement-exception-detail` | Selected exception detail |
| `procurement-exception-link-pr` | Follow-up PR select |
| `procurement-exception-save-links-{exceptionId}` | Save PR/PO links |
| `procurement-exception-linked-actions` | Linked actions summary |

No panel code changes required.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-close-after-link-pr-resolve-journey-smoke.spec.ts` | `/purchasing` | Template → investigate → link PR → save → resolve → close with API asserts |

No assign/waive/cancel mutations. Uses follow-up PR fixture (same category as W311).

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionCloseAfterLinkPrResolveJourneyFixture`
- `assertSupplyArrProcurementExceptionClosedAfterLinkPrResolveFromHandoff`

Reuses W311 `createMinimalPurchaseRequestForExceptions` + `createProcurementExceptionForPurchaseRequest` internals and W313 closed-status assert patterns.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionCloseAfterLinkPrResolveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w314`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-close-after-link-pr-resolve-journey-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-close-after-link-pr-resolve-journey-smoke.spec.ts
```

## Out of scope

- Post-cancel reopen (no API)
- PO link resolve path (covered by W312; close-after-link-PO is a separate slice)
- Waive/close path (covered by W304)
- Escalation or notification process-batch journeys (W301/W302)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception close-after-link-PO-resolve journey smoke (W312 + close), RoutArr product-admin smokes, or post-cancel reopen only if API gains reopen support
