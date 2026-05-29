# W310 — M13 Playwright: SupplyArr procurement exception assign-and-link journey (W250 follow-up)

Builds on **W250** (procurement exception resolution depth — assign resolver + PR/PO link actions), **W295** (procurement exceptions panel smoke).

Post-cancel **reopen** is out of scope — no reopen API exists in SupplyArr procurement exceptions.

Adds **journey** Playwright smoke that seeds an **unassigned** open exception on a purchase request plus a separate follow-up PR, navigates suite handoff → `/purchasing` `procurement-exceptions-panel`, selects the exception detail, clicks **Assign to me**, selects a follow-up PR, clicks **Save PR/PO links**, then verifies assigned resolver + linked PR in UI and API.

## Scope

### Frontend (`ProcurementExceptionsPanel`)

Added stable test ids for W310:

| Test id | Element |
|---------|---------|
| `procurement-exception-link-pr` | Follow-up PR select |
| `procurement-exception-link-po` | Follow-up PO select |
| `procurement-exception-save-links-{exceptionId}` | Save PR/PO links button |
| `procurement-exception-linked-actions` | Linked actions summary |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-assign-link-journey-smoke.spec.ts` | `/purchasing` | Detail select → assign → link PR → save with API asserts |

No investigate/resolve/waive/cancel mutations. No PO link (requires issued PO fixture).

### `e2eApi` helpers

Added:

- `getSupplyArrMeFromHandoff`
- `ensureSupplyArrProcurementExceptionAssignLinkJourneyFixture`
- `assertSupplyArrProcurementExceptionAssignedAndLinkedFromHandoff`

Extended `SupplyArrProcurementExceptionDetail` with assignment + link fields.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionAssignLinkJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w310`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-assign-link-journey-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-assign-link-journey-smoke.spec.ts
```

## Out of scope

- Post-cancel reopen (no API)
- PO link action (needs issued PO journey fixture)
- Escalation or notification process-batch journeys (W301/W302)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception resolve-with-linked-PR journey smoke (investigate → link PR → resolve with template), or next RoutArr product-admin smokes per milestone plan
