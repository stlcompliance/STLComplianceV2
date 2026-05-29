# W309 — M13 Playwright: SupplyArr procurement exception post-reject resolve journey (W306 follow-up)

Builds on **W250** (procurement exception resolution depth), **W303** (investigate/resolve journey smoke), **W306** (reject-waive journey smoke).

Adds **journey** Playwright smoke that seeds a single open exception on a purchase request, navigates suite handoff → `/purchasing` `procurement-exceptions-panel`, clicks **Investigate**, fills waive justification, clicks **Request waive**, verifies `waive_pending`, clicks **Reject waive**, verifies status returns to `investigating` with `waiveRejectionReason`, selects **PR resubmit** resolution template, clicks **Resolve**, then verifies `resolved` status in UI + API with matching `resolutionTemplateKey` and preserved `waiveRejectionReason`.

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exception-post-reject-resolve-journey-smoke.spec.ts` | `/purchasing` | Subject select → template pick → investigate → request waive → reject waive → resolve with API asserts |

No approve-waive/close/cancel mutations. No escalation or notification process-batch.

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionPostRejectResolveJourneyFixture`
- `assertSupplyArrProcurementExceptionPostRejectResolvedWithTemplateFromHandoff`

Reuses W306 waive justification + default reject reason constants and W303 PR resubmit template key.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionPostRejectResolveJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w309`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exception-post-reject-resolve-journey-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin with `canCreatePr` and `canApprovePr` for panel visibility and reject-waive approval path.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exception-post-reject-resolve-journey-smoke.spec.ts
```

## Out of scope

- Post-resolve reopen or waive/close journeys
- Escalation or notification process-batch journeys (W301/W302)
- Resolve without prior reject-waive path (covered by W303)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception assign-and-link journey smoke (W250 resolver + PR link depth), or next SupplyArr/RoutArr product-admin smokes per milestone plan
