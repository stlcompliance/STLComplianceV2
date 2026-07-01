# LedgArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `ledgarr` |
| Category | ERP / finance bridge |
| Entry release | R11 — LedgArr bridge-first finance |
| Completion release | R11 — LedgArr bridge-first finance |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Legal entities, books, dimensions, financial packet governance, posting rules, AP/AR/inventory valuation snapshots, and external ERP bridges. |
| Roadmap slice | Bridge-first finance after operating loops produce trustworthy packets |
| Must not violate | Start bridge-first; do not absorb operating truth or become a full ERP gravity well prematurely. |
| Feature rows retained | 75 |
| Workflow rows retained | 20 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R11 | LedgArr bridge-first finance | 40 | 20 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R11 unless they are only supporting another release gate.
- Common category baseline remains retained for R11.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/ledgarr/FEATURESET.md)
- [Workflow catalog](../../products/ledgarr/WORKFLOWS.md)
- [Product manifest](../../products/ledgarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: Clear for the current auth/session, durable-database guard, and formerly placeholder-success slice.

Completed in this pass:

- Replaced redeemed/source launch-key truth with a fixed ordinary-suite launch catalog for LedgArr handoff and `/api/v1/session` responses.
- Removed `HasLedgArrAccess` / `hasLedgArrAccess` from live API and frontend session contracts so product availability is not represented as an entitlement grant.
- Blocked the production in-memory database fallback: LedgArr now requires a durable database connection outside the Testing environment.
- Converted scaffold-only finance endpoints that returned fake success (`reserved`, `active`, `mapped`, empty success lists) into truthful `501 Not Implemented` responses that state no financial record was created or changed.
- Preserved server-side finance role checks for workspace and payroll actions.

Files touched:

- `apps/ledgarr-api/LedgArr.Api/Endpoints/AuthEndpoints.cs`
- `apps/ledgarr-api/LedgArr.Api/Endpoints/LedgArrEndpoints.cs`
- `apps/ledgarr-api/LedgArr.Api/LedgArrServiceRegistration.cs`
- `apps/ledgarr-api/LedgArr.Api/Services/HandoffAuthService.cs`
- `apps/ledgarr-api/LedgArr.Api/Services/LedgArrStore.cs`
- `apps/ledgarr-api/LedgArr.Api/Services/LedgArrSuiteLaunchCatalog.cs`
- `apps/ledgarr-frontend/src/App.test.tsx`
- `apps/ledgarr-frontend/src/api/client.ts`
- `tests/STLCompliance.LedgArr.Tests/LedgArrAuthEndpointsTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --filter "FullyQualifiedName~LedgArrAuthEndpointsTests" --logger "console;verbosity=minimal"` — passed 6 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.
- `npm test -- App.test.tsx sessionStorage.test.ts` in `apps/ledgarr-frontend` — passed 2 files / 4 tests.
- Current repo-state rerun: `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --no-build --filter "FullyQualifiedName~LedgArrAuthEndpointsTests|FullyQualifiedName~LedgArrTenantSettingsServiceTests|FullyQualifiedName~LedgArrStoreTests.R11_ap_ar_and_inventory_workflows_replace_placeholder_success_paths" --logger "console;verbosity=minimal"` — passed 11 tests in 5s.
- Current repo-state rerun: `npm test -- --run App.test.tsx sessionStorage.test.ts` in `apps/ledgarr-frontend` — passed 2 files / 6 tests in 14.81s.

Remaining R0 blockers:

- No R0 blockers remain in the current LedgArr auth/session, durable-database guard, and formerly placeholder-success slice. Broader R11 finance workflow hardening remains governed by the retained feature and workflow catalogs and must not be pulled into R0.
- No R1/R11 feature expansion was started in this R0 pass.

## R1 Foundation spine pass

Status: Not applicable. LedgArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R11.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no LedgArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no LedgArr rows for `R1`.
- LedgArr's product FEATURESET and WORKFLOWS remain retained full scope, but they do not authorize starting the R11 bridge-first finance stage during the R1 suite stage.

Files touched:

- `docs/roadmap/products/ledgarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no LedgArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. LedgArr must wait for the suite to reach R11 before finance workflow hardening begins.

R1 stage result: LedgArr is clear for the R1 suite gate as not applicable.

## R11 LedgArr bridge-first finance pass

Status: Clear for R11. LedgArr is the only applicable product in this suite stage, and the R0-documented finance workflow blockers have been replaced with durable LedgArr-owned bridge records, server-side finance authorization, tenant-scoped persistence, audit/status history, and focused regression coverage.

Completed in this pass:

- Implemented durable source dimension mapping list/create flows and wired them into dimension resolution without copying another product's source truth.
- Implemented posting-rule list/create/update/activate/deactivate flows against LedgArr-owned posting rule tables.
- Implemented financial-packet rejection with required reason, status history, audit event, and posted-packet protection.
- Implemented integration account-mapping resolution for configured external finance systems and GL accounts.
- Implemented AP bill dispute records and status movement for open vendor bills.
- Implemented AR credit memo creation and customer statement summaries that account for invoices, payments, and credit memos.
- Implemented inventory valuation item and movement listings over LedgArr valuation profiles, cost layers, and valuation movements.
- Preserved bridge-first ownership boundaries: source products remain owners of vendors, customers, assets, work orders, orders, shipments, items, and supporting documents; LedgArr stores finance mappings, packet controls, valuation, posting, and external bridge records.

Files touched:

- `apps/ledgarr-api/LedgArr.Api/Endpoints/LedgArrEndpoints.cs`
- `apps/ledgarr-api/LedgArr.Api/Services/LedgArrStore.cs`
- `tests/STLCompliance.LedgArr.Tests/LedgArrAuthEndpointsTests.cs`
- `tests/STLCompliance.LedgArr.Tests/LedgArrStoreTests.cs`
- `tests/STLCompliance.LedgArr.Tests/LedgArrTenantSettingsServiceTests.cs`
- `docs/roadmap/products/ledgarr.md`
- `docs/roadmap/releases/r11-ledgarr-bridge-first-finance.md`

Tests run:

- `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --logger "console;verbosity=minimal"` — passed 36 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.
- `npm test` in `apps/ledgarr-frontend` — passed 4 files / 9 tests.
- `npm run test:theme` in `apps/ledgarr-frontend` — no theme audit violations.
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter "FullyQualifiedName~LedgArr" --logger "console;verbosity=minimal"` — no tests matched the LedgArr filter; build completed with the same existing warnings.
- `rg` scan over LedgArr API/tests/docs confirmed no live LedgArr API `501`/`NotImplementedFinanceWorkflow` placeholders remain; only historical R0 roadmap notes mention the former blocker state.
- Current repo-state rerun: `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --no-build --filter "FullyQualifiedName~LedgArrAuthEndpointsTests|FullyQualifiedName~LedgArrTenantSettingsServiceTests|FullyQualifiedName~LedgArrStoreTests.R11_ap_ar_and_inventory_workflows_replace_placeholder_success_paths" --logger "console;verbosity=minimal"` — passed 11 tests in 3s.
- Current repo-state rerun: `npm test -- --run App.test.tsx sessionStorage.test.ts` in `apps/ledgarr-frontend` — passed 2 files / 6 tests in 2.96s.

Remaining blockers:

- None for R11.
- R12 retains advanced finance expansion such as deeper ERP synchronization, expanded tax/localization depth, portals, AI-assisted finance review, and category-depth integrations unless a later roadmap gate pulls them forward.

R11 stage result: LedgArr is clear for the R11 suite gate.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for the current R12 trust and UI-scope pass, with advanced finance expansion retained as deferred roadmap scope. No later-stage work was started.

R12 scope audited:

- LedgArr has 35 R12 feature rows and no R12 workflow rows in the roadmap rollout maps.
- R12 rows retain advanced finance depth for enterprise controls with SMB usability, explainable postings, finance inboxes, operational-to-financial reconciliation, external accountant collaboration, continuous close, no-code posting configuration, cash forecasting, multi-entity finance, inventory valuation explanation, correction-not-deletion workflows, portable archive, consolidation/eliminations, planning/scenario modeling, treasury/liquidity, revenue recognition, lease accounting, automated close, anomaly/fraud detection, continuous audit/control monitoring, advanced cost accounting, global tax/e-invoicing, and foundation behavior.
- Current slice audited: LedgArr source-product badges, workspace bootstrap copy, bridge-first finance boundaries, and R11 durable workflow status.

Completed R12 fixes:

- Render source-product badges with canonical suite product names, such as `OrdArr`, instead of generic title-casing like `Ordarr`.
- Removed local runtime plumbing from the normal LedgArr workspace bootstrap panel, replacing API base and frontend port details with session/runtime readiness copy.
- Preserved the bridge-first boundary: LedgArr continues to reference source-product records for finance packets and controls without becoming the source of operational truth.

Files touched:

- `apps/ledgarr-frontend/src/App.tsx`
- `apps/ledgarr-frontend/src/App.test.tsx`
- `docs/roadmap/products/ledgarr.md`

Tests run:

- `npm test -- App.test.tsx sessionStorage.test.ts navigation/ledgarrNav.test.ts pages/settings/LedgArrSettingsPage.test.tsx` in `apps/ledgarr-frontend` — passed 4 files / 11 tests.
- `npm run test:theme` in `apps/ledgarr-frontend` — no theme audit violations.
- `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --logger "console;verbosity=minimal"` — passed 36 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.
- `rg` sweep over the touched LedgArr frontend files found only negative test assertions for removed local runtime copy and generic product casing.

Deferred R12 blockers:

- External accountant portal, one finance inbox, deeper operational-to-financial reconciliation, continuous close, no-code posting and mapping simulation, cash forecasting and scenarios, portable archive, advanced consolidation, planning and scenario modeling, treasury/liquidity, revenue recognition, lease accounting, automated close, anomaly/fraud detection, continuous audit/control monitoring, advanced cost accounting, global tax/e-invoicing, and AI-assisted finance review remain retained R12 scope.
- R11 partial workflow follow-ons such as payment proposal/reconciliation depth, budgets/forecasting, and tax filing are not expanded into advanced R12 capabilities in this pass.

R12 stage result: LedgArr is clear for the R12 suite gate with the deferred advanced finance blockers above.
