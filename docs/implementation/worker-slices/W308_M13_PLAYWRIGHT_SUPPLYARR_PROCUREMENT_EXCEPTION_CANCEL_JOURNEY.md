# W308 — M13 Playwright: SupplyArr procurement exception cancel journey (W306 follow-up)

Builds on **W250** (procurement exception resolution depth), **W303** (investigate/resolve journey smoke), **W306** (reject-waive journey smoke).

Adds **journey** Playwright smoke that seeds a single open exception on a purchase request, navigates suite handoff → `/purchasing` `procurement-exceptions-panel`, clicks **Investigate**, fills cancel reason, clicks **Cancel**, then verifies `cancelled` status in UI + API with matching `cancellationReason` and `cancelledAt`, and workflow controls hidden.

## Scope

### API (`apps/supplyarr-api`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionResponse` | Expose `cancelledAt` + `cancellationReason` on GET/list responses for journey API asserts |
| `ProcurementExceptionService.MapEntity` | Map cancellation fields from entity |

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionsPanel` | `procurement-exception-cancel-reason` textarea, `procurement-exception-cancel-{id}` test id, cancelled status badge styling |
| `ProcurementExceptionsPanel.test.tsx` | Vitest asserts cancel reason field + cancel button test id on investigating row |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-cancel-journey-smoke.spec.ts` | `/purchasing` | Subject select → investigate → fill cancel reason → cancel with API asserts |

No waive/resolve/close mutations. No escalation or notification process-batch.

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionCancelJourneyFixture`
- `supplyArrProcurementExceptionCancelJourneyReason`
- `assertSupplyArrProcurementExceptionCancelledWithReasonFromHandoff`

Extended `SupplyArrProcurementExceptionDetail` with `cancelledAt` + `cancellationReason`.

Reuses W303/W295 PR + exception seed internals.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionCancelJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w308`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-cancel-journey-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin with `canCreatePr` for panel visibility.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- ProcurementExceptionsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-cancel-journey-smoke.spec.ts
```

## Out of scope

- Post-cancel reopen or resolve journeys
- Escalation or notification process-batch journeys (W301/W302)
- Open exception cancel without investigate step (cancel only allowed from investigating)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception post-reject resolve journey smoke (W306 follow-up: investigate → reject waive → resolve with template), or next SupplyArr/RoutArr product-admin smokes per milestone plan
