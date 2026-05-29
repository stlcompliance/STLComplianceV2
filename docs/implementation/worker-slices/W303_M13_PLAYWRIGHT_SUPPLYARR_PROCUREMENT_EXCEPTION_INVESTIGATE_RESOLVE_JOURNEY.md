# W303 — M13 Playwright: SupplyArr procurement exception investigate/resolve journey (W295 follow-up)

Builds on **W250** (procurement exception resolution depth + templates), **W295** (procurement exceptions panel read-only smoke), **W302** (procurement notification process-batch journey).

Adds **journey** Playwright smoke that seeds a single open exception on a purchase request, navigates suite handoff → `/purchasing` `procurement-exceptions-panel`, selects **PR resubmit** resolution template, clicks **Investigate**, verifies `investigating` status in UI + API, clicks **Resolve**, then verifies `resolved` status and template key in UI + API.

## Scope

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionsPanel` | `procurement-exception-status-{exceptionId}` test id on status badge for journey assertions |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-investigate-resolve-journey-smoke.spec.ts` | `/purchasing` | Subject select → template → investigate → resolve with API asserts |

No waive/close/cancel mutations. No escalation or notification process-batch.

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionInvestigateResolveJourneyFixture`
- `getSupplyArrProcurementException` / `FromHandoff`
- `assertSupplyArrProcurementExceptionStatus` / `FromHandoff`
- `assertSupplyArrProcurementExceptionResolvedWithTemplateFromHandoff`

Reuses W295 `createMinimalPurchaseRequestForExceptions` + `createProcurementExceptionForPurchaseRequest` internals.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionInvestigateResolveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w303`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-investigate-resolve-journey-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-investigate-resolve-journey-smoke.spec.ts
```

## Out of scope

- Waive request/approve/close workflow Playwright mutations
- Escalation or notification process-batch journeys (W301/W302)
- RoutArr notification re-enable-with-new-webhook reload persistence (W300 follow-up)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch notification re-enable-with-new-webhook reload persistence smoke (W300 follow-up), or SupplyArr procurement exception waive/close journey smoke (W303 follow-up)
