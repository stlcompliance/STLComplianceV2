# W306 — M13 Playwright: SupplyArr procurement exception reject-waive journey (W304 follow-up)

Builds on **W250** (procurement exception resolution depth), **W304** (waive/close journey smoke).

Adds **journey** Playwright smoke that seeds a single open exception on a purchase request, navigates suite handoff → `/purchasing` `procurement-exceptions-panel`, clicks **Investigate**, fills waive justification, clicks **Request waive**, verifies `waive_pending` in UI + API, clicks **Reject waive**, then verifies status returns to `investigating`, `waiveRejectionReason` in API matches panel default, and **Resolve** / **Request waive** controls are visible again (approve/reject waive hidden).

## Scope

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionsPanel` | `procurement-exception-reject-waive-{id}` test id on waive_pending reject button |
| `ProcurementExceptionsPanel.test.tsx` | Vitest asserts reject-waive test id on waive_pending row |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-reject-waive-journey-smoke.spec.ts` | `/purchasing` | Subject select → investigate → request waive → reject waive with API asserts |

No approve-waive/close/cancel mutations. No escalation or notification process-batch.

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionRejectWaiveJourneyFixture`
- `supplyArrProcurementExceptionRejectWaiveJourneyJustification`
- `supplyArrProcurementExceptionRejectWaiveDefaultReason`
- `assertSupplyArrProcurementExceptionRejectedWaiveFromHandoff`

Extended `SupplyArrProcurementExceptionDetail` with `waiveRejectionReason`.

Reuses W304/W295 PR + exception seed internals.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionRejectWaiveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w306`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-reject-waive-journey-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin with `canCreatePr` and `canApprovePr` for panel visibility and reject-waive approval path.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- ProcurementExceptionsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-reject-waive-journey-smoke.spec.ts
```

## Out of scope

- Cancel workflow Playwright mutations
- Post-reject resolve journey (separate follow-up)
- Escalation or notification process-batch journeys (W301/W302)
- RoutArr disable-save-then-re-enable reload persistence (W305 follow-up)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch notification disable-save-then-re-enable reload persistence smoke (W305 follow-up), or SupplyArr procurement exception cancel journey smoke (W306 follow-up)
