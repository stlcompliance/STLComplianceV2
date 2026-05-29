# W311 — M13 Playwright: SupplyArr procurement exception investigate → link PR → resolve-with-template journey (W250/W303/W310 follow-up)

Builds on **W250** (procurement exception resolution depth — link PR + resolution templates), **W303** (investigate → resolve journey), **W310** (assign → link PR journey).

Completes W250 **link + resolve** depth in one browser flow: open fixture → **Investigate** → link follow-up PR → **Save PR/PO links** → **Resolve** with PR resubmit template.

## Scope

### Frontend (`ProcurementExceptionsPanel`)

Reuses existing test ids from W303/W310:

| Test id | Element |
|---------|---------|
| `procurement-exception-resolution-template` | Resolution template select |
| `procurement-exception-status-{exceptionId}` | Status badge |
| `procurement-exception-investigate-{exceptionId}` | Investigate action |
| `procurement-exception-resolve-{exceptionId}` | Resolve action |
| `procurement-exception-key-{exceptionId}` | Open detail |
| `procurement-exception-detail` | Selected exception detail |
| `procurement-exception-link-pr` | Follow-up PR select |
| `procurement-exception-save-links-{exceptionId}` | Save PR/PO links |
| `procurement-exception-linked-actions` | Linked actions summary |

No panel code changes required.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-investigate-link-resolve-journey-smoke.spec.ts` | `/purchasing` | Template → investigate → link PR → save → resolve with API asserts |

No assign/waive/cancel mutations. No PO link (requires issued PO fixture).

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionInvestigateLinkResolveJourneyFixture`
- `assertSupplyArrProcurementExceptionInvestigateLinkResolvedFromHandoff`

Reuses W295/W310 `createMinimalPurchaseRequestForExceptions` + `createProcurementExceptionForPurchaseRequest` internals.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionInvestigateLinkResolveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w311`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-investigate-link-resolve-journey-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-investigate-link-resolve-journey-smoke.spec.ts
```

## Out of scope

- Post-cancel reopen (no API)
- PO link action (needs issued PO journey fixture)
- Assign-to-me step (covered by W310)
- Escalation or notification process-batch journeys (W301/W302)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception resolve-with-PO-link journey smoke (issued PO fixture), RoutArr product-admin smokes, or post-cancel reopen only if API gains reopen support
