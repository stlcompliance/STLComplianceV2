# W315 — M13 Playwright: SupplyArr procurement exception investigate → link PO → resolve → close journey (W312/W313 follow-up)

Builds on **W250** (procurement exception resolution depth + templates), **W312** (investigate → link PO → resolve journey), **W313** (close-after-resolve journey), **W314** (close-after-link-PR-resolve journey pattern).

Completes the link-PO resolve path lifecycle in one browser flow: open fixture → **Investigate** → link issued follow-up PO → **Save PR/PO links** → **Resolve** with PO reissue template → **Close** with API asserts for linked PO, resolved template, closed status, and `closedAt`.

## Scope

### Frontend (`ProcurementExceptionsPanel`)

Reuses existing test ids from W303/W310/W312/W313:

| Test id | Element |
|---------|---------|
| `procurement-exception-resolution-template` | Resolution template select |
| `procurement-exception-status-{exceptionId}` | Status badge |
| `procurement-exception-investigate-{exceptionId}` | Investigate action |
| `procurement-exception-resolve-{exceptionId}` | Resolve action |
| `procurement-exception-close-{exceptionId}` | Close action (visible when resolved) |
| `procurement-exception-key-{exceptionId}` | Open detail |
| `procurement-exception-detail` | Selected exception detail |
| `procurement-exception-link-po` | Follow-up PO select |
| `procurement-exception-save-links-{exceptionId}` | Save PR/PO links |
| `procurement-exception-linked-actions` | Linked actions summary |

No panel code changes required.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-close-after-link-po-resolve-journey-smoke.spec.ts` | `/purchasing` | Template → investigate → link issued PO → save → resolve → close with API asserts |

No assign/waive/cancel mutations. Uses issued PO fixture (same category as W312).

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionCloseAfterLinkPoResolveJourneyFixture`
- `assertSupplyArrProcurementExceptionClosedAfterLinkPoResolveFromHandoff`

Reuses W312 `createIssuedPurchaseOrderForExceptions` + W313 closed-status assert patterns.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionCloseAfterLinkPoResolveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w315`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-close-after-link-po-resolve-journey-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin with procurement exception manage permission and PO issue authority.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- --run ProcurementExceptionsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-close-after-link-po-resolve-journey-smoke.spec.ts
```

## Out of scope

- Post-cancel reopen (no API)
- PR link resolve path (covered by W311/W314)
- Waive/close path (covered by W304)
- Escalation or notification process-batch journeys (W301/W302)

## Next recommended slice

- **M13 Playwright** — RoutArr product-admin smokes, SupplyArr procurement exception post-cancel reopen only if API gains reopen support, or next RoutArr dispatch/notification depth slice per milestone plan
