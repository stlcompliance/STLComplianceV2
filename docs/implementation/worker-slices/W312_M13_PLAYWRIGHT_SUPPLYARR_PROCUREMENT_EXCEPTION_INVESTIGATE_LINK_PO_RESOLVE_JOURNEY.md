# W312 — M13 Playwright: SupplyArr procurement exception investigate → link PO → resolve-with-template journey (W250/W311/W310 follow-up)

Builds on **W250** (procurement exception resolution depth — link PR/PO + resolution templates), **W311** (investigate → link PR → resolve journey), **W310** (assign → link PR journey).

Completes W250 **PO link + resolve** depth in one browser flow: open fixture → **Investigate** → link issued follow-up PO → **Save PR/PO links** → **Resolve** with PO reissue template.

## Scope

### Frontend (`ProcurementExceptionsPanel`)

Reuses existing test ids from W310/W311:

| Test id | Element |
|---------|---------|
| `procurement-exception-resolution-template` | Resolution template select |
| `procurement-exception-status-{exceptionId}` | Status badge |
| `procurement-exception-investigate-{exceptionId}` | Investigate action |
| `procurement-exception-resolve-{exceptionId}` | Resolve action |
| `procurement-exception-key-{exceptionId}` | Open detail |
| `procurement-exception-detail` | Selected exception detail |
| `procurement-exception-link-po` | Follow-up PO select |
| `procurement-exception-save-links-{exceptionId}` | Save PR/PO links |
| `procurement-exception-linked-actions` | Linked actions summary |

No panel code changes required.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-investigate-link-po-resolve-journey-smoke.spec.ts` | `/purchasing` | Template → investigate → link issued PO → save → resolve with API asserts |

No assign/waive/cancel mutations. Uses issued PO fixture (submit/approve PR → create/approve/issue PO).

### `e2eApi` helpers

Added:

- `createIssuedPurchaseOrderForExceptions` (internal)
- `ensureSupplyArrProcurementExceptionInvestigateLinkPoResolveJourneyFixture`
- `assertSupplyArrProcurementExceptionInvestigateLinkPoResolvedFromHandoff`

Reuses W295/W310 `createMinimalPurchaseRequestForExceptions` + `createProcurementExceptionForPurchaseRequest` internals.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionInvestigateLinkPoResolveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w312`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-investigate-link-po-resolve-journey-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-investigate-link-po-resolve-journey-smoke.spec.ts
```

## Out of scope

- Post-cancel reopen (no API)
- PR link path (covered by W311)
- Assign-to-me step (covered by W310)
- Escalation or notification process-batch journeys (W301/W302)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception close-after-link-PR-resolve journey smoke (W311 + close), RoutArr product-admin smokes, or post-cancel reopen only if API gains reopen support
