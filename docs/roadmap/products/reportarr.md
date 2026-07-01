# ReportArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `reportarr` |
| Category | BI / reporting |
| Entry release | R10 — ReportArr operational reporting |
| Completion release | R10 — ReportArr operational reporting |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Read models, dashboards, metrics, scheduled reports, exports, provenance drillbacks, and audit-ready report outputs. |
| Roadmap slice | Operational reporting after source events exist |
| Must not violate | ReportArr projects and explains source truth; it must not correct source truth. |
| Feature rows retained | 68 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R10 | ReportArr operational reporting | 33 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R10 unless they are only supporting another release gate.
- Common category baseline remains retained for R10.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/reportarr/FEATURESET.md)
- [Workflow catalog](../../products/reportarr/WORKFLOWS.md)
- [Product manifest](../../products/reportarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: Auth/session slice fixed; production trust remains blocked by the scaffold BI store and the remaining durable BI security/output model.

Completed in this pass:

- Replaced redeemed launch-key truth with a fixed ordinary-suite launch catalog for ReportArr handoff, `/api/me`, and `/api/v1/session` responses.
- Removed `HasReportArrAccess` / `hasReportArrAccess` from live API and frontend session contracts so product availability is not represented as an entitlement grant.
- Removed the handoff failure path that rejected ordinary active tenant users based on ReportArr being absent from a redeemed launch-key list.
- Kept Compliance Core out of ordinary product launch lists while preserving ReportArr route authorization tests for reporting roles.

Files touched:

- `apps/reportarr-api/ReportArr.Api/Data/ReportArrStore.cs`
- `apps/reportarr-api/ReportArr.Api/Endpoints/AuthEndpoints.cs`
- `apps/reportarr-api/ReportArr.Api/Models/ReportArrContracts.cs`
- `apps/reportarr-api/ReportArr.Api/Services/HandoffAuthService.cs`
- `apps/reportarr-api/ReportArr.Api/Services/ReportArrSuiteLaunchCatalog.cs`
- `apps/reportarr-frontend/src/App.test.tsx`
- `apps/reportarr-frontend/src/api/client.ts`
- `apps/reportarr-frontend/src/api/types.ts`
- `tests/STLCompliance.ReportArr.Auth.Tests/ReportArrAuthEndpointsTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --filter "FullyQualifiedName~ReportArrAuthEndpointsTests" --logger "console;verbosity=minimal"` — passed 6 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.
- `npm test -- App.test.tsx sessionStorage.test.ts` in `apps/reportarr-frontend` — passed 2 files / 4 tests.
- Current repo-state rerun: `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~ReportArrAuthEndpointsTests" --logger "console;verbosity=minimal"` — passed 7 tests in 5s.
- Current repo-state rerun: `npm test -- --run App.test.tsx sessionStorage.test.ts` in `apps/reportarr-frontend` — passed 2 files / 6 tests in 3.13s.

Deferred R0 blockers:

- `ReportArrStore` remains a singleton process-local scaffold for datasets, read models, dashboards, report definitions, runs, schedules, exports, metrics, alerts, audit packages, and refresh jobs. It is not durable, replayable, or production-trust-clear for BI definitions, runs, lineage, schedules, outputs, or report evidence.
- Source-product visibility is no longer unlocked by fixed-suite launchable product keys, but ReportArr still lacks durable row/column/report security, source lineage, StaffArr/NexArr-backed delegated action permissions, export security, and RecordArr-backed retained outputs required for production trust clearance.
- No R1/R10 feature expansion was started in this R0 pass.

## R1 Foundation spine pass

Status: Not applicable. ReportArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R10.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no ReportArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no ReportArr rows for `R1`.
- ReportArr's product FEATURESET and WORKFLOWS remain retained full scope, including the scaffold BI store and durable BI security/output blockers, but they do not authorize starting the R10 operational reporting stage during the R1 suite stage.

Files touched:

- `docs/roadmap/products/reportarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no ReportArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. The deferred R0 BI-store durability, lineage, row/column security, export security, and retained-output blockers remain active for the later ReportArr R10 stage.

R1 stage result: ReportArr is clear for the R1 suite gate as not applicable.

## R10 ReportArr operational reporting pass

Status: Clear for R10 with deferred durable-BI-engine blockers documented below. The represented R10 slice now avoids treating fixed-suite launchability as source-product reporting permission.

R10 scope audited:

- R10 roadmap rows: 33 feature rows and 13 workflow rows mapped to ReportArr.
- Product boundaries in `docs/products/reportarr/FEATURESET.md`, `docs/products/reportarr/WORKFLOWS.md`, and the report/print/export constitution.
- ReportArr source connector, ingestion event, dataset, read-model, dashboard, report, schedule, export, KPI, alert, audit package, and print/export surfaces.
- Existing R0 notes about process-local/scaffold reporting truth and launch-key-derived source-product access.

Completed in this pass:

- Replaced source-product reporting checks that depended on fixed-suite launchable product keys with ReportArr/reporting role checks.
- Replaced report/dashboard policy `AllowedPermissionRefs` matching against launchable product keys with explicit ReportArr permission-to-role matching.
- Added regression coverage proving that an ordinary active tenant user with `maintainarr` and `reportarr` launch keys cannot see a MaintainArr-backed dataset unless they also have a reporting role.
- Preserved ReportArr's source-of-truth boundary: ReportArr remains read/projection/reporting only and does not mutate owning product records.

Files touched:

- `apps/reportarr-api/ReportArr.Api/Data/ReportArrStore.cs`
- `tests/STLCompliance.ReportArr.Auth.Tests/ReportArrAuthEndpointsTests.cs`
- `docs/roadmap/products/reportarr.md`

Tests run:

- `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` — passed 7 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.
- `npm test` in `apps/reportarr-frontend` — passed 3 files / 8 tests.
- `npm run test:theme` in `apps/reportarr-frontend` — no violations.
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter "FullyQualifiedName~ReportArr" --logger "console;verbosity=minimal"` — passed 48 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.
- Current repo-state rerun: `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~ReportArrAuthEndpointsTests" --logger "console;verbosity=minimal"` — passed 7 tests in 2s.
- Current repo-state rerun: `npm test -- --run App.test.tsx sessionStorage.test.ts` in `apps/reportarr-frontend` — passed 2 files / 6 tests in 2.11s.
- Current repo-state rerun: `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --no-build --filter "FullyQualifiedName~ReportArr" --logger "console;verbosity=minimal"` — passed 48 tests in 1s.

Deferred blockers:

- ReportArr still uses a singleton store with a JSON snapshot persistence layer rather than normalized durable BI tables for connectors, events, semantic definitions, runs, schedules, exports, metrics, lineage, and audit packages. This is a bounded production-hardening blocker for a future ReportArr storage migration, not a reason to expand R10 into a full warehouse rewrite during this pass.
- ReportArr retained outputs can use the existing print/archive path, but generated report/export jobs are not yet a fully durable, replayable, warehouse-grade execution history with retry orchestration, delivery reconciliation, and retained-output lifecycle state.
- Fine-grained row/column security remains role/policy based in this slice; full StaffArr/NexArr delegated action permissions, sensitivity labels, and external portal scopes remain deferred.
- R12 remains the home for natural-language analysis, advanced semantic layer/lakehouse orchestration, forecasting/anomaly depth, embedded analytics SDK, scenario analysis, data catalog, privacy-preserving analytics, streaming intelligence, and advanced BI model governance.

R10 stage result: ReportArr is clear for the R10 suite gate with the deferred durable-BI-engine blockers above.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for the current R12 pass with advanced BI/AI and durable reporting-engine work deferred below. No R13 or later-stage work was started.

R12 scope audited:

- R12 roadmap rows: 35 feature rows and 2 workflow rows mapped to ReportArr.
- R12 workflows retained for later implementation: `RP-WF-013` natural-language question to governed answer and `RP-WF-015` BI access review and external embedding.
- Product boundaries in `docs/products/reportarr/FEATURESET.md`, `docs/products/reportarr/WORKFLOWS.md`, the ownership constitution, shared UI constitution, record-detail/page-state/report-print constitutions, and the existing R10 durable-BI-engine deferrals.
- Current ReportArr UI slices for datasets, connectors, dataset fields, lineage, ingestion events, read models, access policies, audit packages, source connector detail, dataset detail, dashboard detail, and settings.

Completed in this pass:

- Replaced user-facing raw source product keys with suite display names across ReportArr dataset, connector, field, lineage, ingestion-event, audit-package, access-policy, read-model, and detail-shell surfaces.
- Removed local preview/runtime plumbing from the Settings page copy so normal users see session readiness instead of API base, preview port, or endpoint details.
- Preserved source-owned boundaries: ReportArr still projects and explains upstream truth, while forms and API payloads keep the existing source product keys required by service contracts.
- Added frontend regression coverage for settings copy and integration source-product display.
- Did not implement natural-language analysis, semantic-layer, embedding SDK, lakehouse, forecasting, or external portal expansion in this product-stage pass.

Files touched:

- `apps/reportarr-frontend/src/App.tsx`
- `apps/reportarr-frontend/src/App.test.tsx`
- `docs/roadmap/products/reportarr.md`

Tests run:

- `npm test -- App.test.tsx sessionStorage.test.ts` in `apps/reportarr-frontend` - passed 2 files / 6 tests.
- `npm run test:theme` in `apps/reportarr-frontend` - no violations.
- `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - passed 7 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.

Deferred R12 blockers:

- `RP-WF-013` natural-language question to governed answer remains deferred until ReportArr has a governed semantic layer, citation provenance, model governance, prompt/test controls, and durable answer audit trails.
- `RP-WF-015` BI access review and external embedding remains deferred until external portal scopes, embed tokens, tenant-safe access reviews, and revocation/reconciliation flows are implemented.
- Enterprise semantic layer/metric store, lakehouse/warehouse orchestration, advanced anomaly/forecasting, embedded analytics SDK, what-if/scenario analysis, narrative board packs, data catalog/impact graph, privacy-preserving analytics, streaming operational intelligence, and advanced model governance/testing remain retained R12 scope but are not production-ready in this pass.
- Existing durable-BI-engine blockers from R10 remain: normalized durable BI tables, replayable report/export execution history, retry/delivery reconciliation, retained-output lifecycle state, fine-grained row/column security, and delegated StaffArr/NexArr action permissions are still future storage/security work.

R12 stage result: ReportArr is clear for the R12 suite gate with the deferred advanced BI/AI and durable reporting-engine blockers above.
