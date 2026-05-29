# W295 — M13 Playwright: SupplyArr procurement exceptions panel smoke (W250)

Builds on **W250** (`ProcurementExceptionsPanel` SLA/templates/resolver depth), **W294** (SupplyArr purchasing Playwright smoke + handoff journey), and **W256** (read-only triage interaction pattern).

## Scope

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionsPanel` | Test ids: `procurement-exception-subject-record`, `procurement-exception-resolution-template`, `procurement-exception-row-*`, `procurement-exception-sla-breached-*`, `procurement-exception-key-*`, `procurement-exception-investigate-*`, `procurement-exception-resolve-*`, `procurement-exception-assign-*` |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-purchasing-procurement-exceptions-smoke.spec.ts` | `/purchasing` | Suite sign-in → handoff → `procurement-exceptions-panel`; resolution template picker to **PR resubmit**; select fixture PR subject; overdue row SLA breached badge; open row investigate button; exception detail with assign control (buttons stay unclicked) |

Investigate/resolve/assign **clicks are out of scope** — read-only operator UX verification (matches W256/W294).

### e2eApi fixture

`ensureSupplyArrProcurementExceptionsFixture()` in `support/e2eApi.ts`:

- Creates vendor, part, draft purchase request via SupplyArr API
- Creates one overdue SLA exception (`slaDueAt` in the past) and one open exception via `POST /api/purchase-requests/{id}/procurement-exceptions`
- Returns `{ purchaseRequestId, requestKey, exceptionIds, overdueExceptionId, openExceptionId }`

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrPurchasingProcurementExceptionsSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w295`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-purchasing-procurement-exceptions-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin with `canCreatePr` for panel visibility.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- ProcurementExceptionsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-purchasing-procurement-exceptions-smoke.spec.ts
```

## Out of scope

- Live investigate/resolve/assign/waive Playwright mutations
- Load-test journey seed endpoint extension (direct API fixture like W256)
- Procurement exception SLA escalation worker / notifications

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch notification settings panel explicit webhook clear on disable smoke (settings-only; optional)
- **SupplyArr M8** — procurement exception SLA escalation worker / notifications (W250 out of scope)
