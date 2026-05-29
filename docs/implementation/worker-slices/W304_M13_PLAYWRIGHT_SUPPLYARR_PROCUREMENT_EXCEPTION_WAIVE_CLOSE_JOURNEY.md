# W304 — M13 Playwright: SupplyArr procurement exception waive/close journey (W303 follow-up)

Builds on **W250** (procurement exception resolution depth), **W295** (procurement exceptions panel read-only smoke), **W303** (investigate/resolve journey smoke).

Adds **journey** Playwright smoke that seeds a single open exception on a purchase request, navigates suite handoff → `/purchasing` `procurement-exceptions-panel`, clicks **Investigate**, fills waive justification, clicks **Request waive**, verifies `waive_pending` in UI + API, clicks **Approve waive**, verifies `waived` + justification in API, clicks **Close**, then verifies `closed` + `closedAt` in API.

## Scope

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionsPanel` | `procurement-exception-waive-justification`, `procurement-exception-request-waive-{id}`, `procurement-exception-approve-waive-{id}`, `procurement-exception-close-{id}` test ids |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-waive-close-journey-smoke.spec.ts` | `/purchasing` | Subject select → investigate → request waive → approve waive → close with API asserts |

No reject-waive/cancel mutations. No escalation or notification process-batch.

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionWaiveCloseJourneyFixture`
- `supplyArrProcurementExceptionWaiveCloseJourneyJustification`
- `assertSupplyArrProcurementExceptionWaivedWithJustificationFromHandoff`
- `assertSupplyArrProcurementExceptionClosedFromHandoff`

Extended `SupplyArrProcurementExceptionDetail` with `waiveJustification` and `closedAt`.

Reuses W303/W295 PR + exception seed internals.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionWaiveCloseJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w304`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-waive-close-journey-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin with `canCreatePr` and `canApprovePr` for panel visibility and waive approval.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- ProcurementExceptionsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-waive-close-journey-smoke.spec.ts
```

## Out of scope

- Reject waive / cancel workflow Playwright mutations
- Escalation or notification process-batch journeys (W301/W302)
- RoutArr notification re-enable-with-new-webhook reload persistence (W300 follow-up; completed in W305)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception reject-waive journey smoke (W304 follow-up), or RoutArr dispatch notification disable-save-then-re-enable reload persistence (W305 follow-up)
