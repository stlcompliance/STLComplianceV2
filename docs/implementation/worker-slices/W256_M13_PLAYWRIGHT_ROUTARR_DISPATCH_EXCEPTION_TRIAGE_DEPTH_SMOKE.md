# W256 — M13 Playwright: RoutArr dispatch exception triage depth smoke (W254)

Builds on **W254** (`DispatchExceptionQueuePanel` SLA/templates/bulk actions), **W243** (exception queue Playwright smoke), and **W253** (read-only mutation pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-exception-triage-depth-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `dispatch-exception-queue-panel`; when triage allowed, `exception-bulk-actions` with resolution template picker; row checkbox selection enables bulk assign/resolve (buttons stay unclicked); template select to **Reschedule departure**; overdue filter toggle + SLA breached badge when `ensureRoutArrDispatchExceptionTriageFixture()` seeds rows |

Bulk assign/resolve **clicks are out of scope** — read-only triage interaction smoke only (matches W243/W253).

### e2eApi fixture

`ensureRoutArrDispatchExceptionTriageFixture()` in `support/e2eApi.ts`:

- Creates one overdue SLA exception (`slaDueAt` in the past) and one open exception via `POST /api/dispatch/exceptions`
- Returns `{ exceptionIds, overdueExceptionId, openExceptionId }`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchExceptionTriageDepthSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w256`
- `All.Count >= 28`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-exception-triage-depth-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo platform admin typically has dispatch triage (`canTriage`).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build packages/shared-dotnet/STLCompliance.Shared/STLCompliance.Shared.csproj -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-dispatch-exception-triage-depth-smoke.spec.ts
```

## Out of scope

- Bulk assign/resolve Playwright mutations
- New triage API or panel features (see W254)
- Automated SLA escalation worker

## Next slice

- **RoutArr M9** — proof/DVIR capture depth follow-ups
- **NexArr M12** — platform-admin service token / worker health orchestration UI (if scoped)
- **M13 Playwright** — unassigned queue preview-before-assign depth smoke (optional, builds on W255)
