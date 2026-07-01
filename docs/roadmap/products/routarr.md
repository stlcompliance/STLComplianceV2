# RoutArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `routarr` |
| Category | TMS |
| Entry release | R8 — Dispatch and transportation execution |
| Completion release | R8 — Dispatch and transportation execution |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Transportation demand, dispatch, routes, trips, stops, driver/equipment snapshots, exceptions, proof, dock visibility, and freight packets. |
| Roadmap slice | Dispatch and transportation execution |
| Must not violate | Dispatch only against explicit readiness snapshots from owning products. |
| Feature rows retained | 73 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R8 | Dispatch and transportation execution | 38 | 15 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R8 unless they are only supporting another release gate.
- Common category baseline remains retained for R8.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/routarr/FEATURESET.md)
- [Workflow catalog](../../products/routarr/WORKFLOWS.md)
- [Product manifest](../../products/routarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: Clear for the current R0 auth/session slice.

Completed in this pass:

- Replaced redeemed launch-key truth with a fixed ordinary-suite launch catalog for RoutArr handoff, `/api/me`, and `/api/v1/session` responses.
- Removed `HasRoutArrAccess` / `hasRoutArrAccess` from live API and frontend session contracts so product availability is not represented as an entitlement grant.
- Renamed the internal authorization helper from entitlement wording to launch-context wording while preserving server-side RoutArr role checks.
- Verified normal tenant users are still accepted after a non-RoutArr launch context and that Compliance Core is not exposed in ordinary product launch lists.

Files touched:

- `apps/routarr-api/RoutArr.Api/Contracts/AuthContracts.cs`
- `apps/routarr-api/RoutArr.Api/Services/HandoffAuthService.cs`
- `apps/routarr-api/RoutArr.Api/Services/MeService.cs`
- `apps/routarr-api/RoutArr.Api/Services/RoutArrAuthorizationService.cs`
- `apps/routarr-api/RoutArr.Api/Services/RoutArrSuiteLaunchCatalog.cs`
- `apps/routarr-frontend/src/api/types.ts`
- `apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx`
- `tests/STLCompliance.RoutArr.Auth.Tests/RoutArrHandoffApiTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrHandoffApiTests" --logger "console;verbosity=minimal"` — passed 6 tests. Existing NuGet pruning and EF Core version-conflict warnings remain.
- `npm test -- ProductWorkspaceLayout.test.tsx sessionStorage.test.ts` in `apps/routarr-frontend` — passed 2 files / 6 tests.

Remaining R0 blockers:

- None identified in the current auth/session slice. Broader R8 dispatch readiness-snapshot authority remains governed by this roadmap and the product workflows; no R1 or later work was started.

## R1 Foundation spine pass

Status: Not applicable. RoutArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R8.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no RoutArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no RoutArr rows for `R1`.
- RoutArr's product FEATURESET and WORKFLOWS remain retained full scope, but they do not authorize starting the R8 dispatch and transportation execution slice during the R1 suite stage.

Files touched:

- `docs/roadmap/products/routarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no RoutArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. RoutArr must wait for the suite to reach R8 before dispatch/transport execution work begins.

R1 stage result: RoutArr is clear for the R1 suite gate as not applicable.

## R8 Dispatch and transportation execution pass

Status: complete for the current RoutArr dispatch and transportation execution baseline.

Files changed:

- `docs/roadmap/products/routarr.md`
- `docs/roadmap/releases/r8-dispatch-and-transportation-execution.md`

Completed R8 verification:

- Verified RoutArr remains the durable owner for transportation demand, dispatch plans, routes, trips, stops, release snapshots, proof, DVIR, exceptions, completion rollups, dock/yard context, carrier/tender context, finance contributions, audit, notifications, and integration outbox records.
- Verified dispatch release/readiness behavior uses explicit source snapshots and owning-product checks instead of copying StaffArr, TrainArr, MaintainArr, Compliance Core, LoadArr, SupplyArr, CustomArr, OrdArr, RecordArr, or LedgArr source truth.
- Verified dispatch assignment blocks and previews remain server-side and tenant-scoped through driver eligibility, asset dispatchability, availability, and Compliance Core workflow gate checks.
- Verified trip execution, proof/DVIR, completion rollup, TMS runtime, dispatch board, exception queue, and closeout surfaces remain covered by R8-critical backend tests.
- Verified the RoutArr frontend dispatch/transportation workspace test suite and theme audit remain green with no new UI work required for this stage.

Tests run:

- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` — timed out after 304 seconds before returning useful results; not counted as pass evidence.
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrDispatchWorkflowGateTests|FullyQualifiedName~RoutArrDispatchAssignmentTests|FullyQualifiedName~RoutArrAssetDispatchabilityTests|FullyQualifiedName~RoutArrDriverEligibilityTests" --logger "console;verbosity=minimal"` — passed 14 tests.
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrTripTests|FullyQualifiedName~RoutArrTripExecutionCaptureTests|FullyQualifiedName~RoutArrTripProofDvirTests|FullyQualifiedName~RoutArrTripCompletionRollupWorkerTests" --logger "console;verbosity=minimal"` — passed 21 tests.
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrTmsRuntimeTests|FullyQualifiedName~RoutArrSupplyArrPartsDemandTests|FullyQualifiedName~RoutArrDispatchBoardTests|FullyQualifiedName~RoutArrDispatchCloseoutTests|FullyQualifiedName~RoutArrDispatchExceptionQueueTests" --logger "console;verbosity=minimal"` — passed 27 tests.
- `npm test` from `apps/routarr-frontend` — passed 40 files / 134 tests.
- `npm run test:theme` from `apps/routarr-frontend` — passed with no theme audit violations.
- Current repo-state rerun: `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~RoutArrHandoffApiTests" --logger "console;verbosity=minimal"` — passed 6 tests in 22s.
- Current repo-state rerun: `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~RoutArrDispatchWorkflowGateTests|FullyQualifiedName~RoutArrDispatchAssignmentTests|FullyQualifiedName~RoutArrAssetDispatchabilityTests|FullyQualifiedName~RoutArrDriverEligibilityTests" --logger "console;verbosity=minimal"` — passed 14 tests in 49s.
- Current repo-state rerun: `npm test -- --run ProductWorkspaceLayout.test.tsx sessionStorage.test.ts` from `apps/routarr-frontend` — passed 2 files / 6 tests in 1.64s.

Known warnings:

- The .NET test runs still emit existing NU1510 health-check package prune warnings and Microsoft.EntityFrameworkCore.Relational 10.0.4/10.0.8 conflict warnings.

Remaining blockers and deferrals:

- No R8 blockers remain for the audited dispatch and transportation execution baseline.
- Full backend project execution should be rerun with a longer timeout or split by established clusters in CI; the broad single-command run exceeded the local tool window.
- R12 optimization, control tower, marketplace/shared-capacity, carbon/alternative-energy planning, autonomous/robotic handoff, automated freight audit/dispute, and advanced document orchestration remain retained roadmap scope.

R8 stage result: RoutArr is clear for the R8 suite gate.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: complete for the current RoutArr R12 trust and UI-scope pass, with advanced expansion scope retained as deferred roadmap work.

R12 scope audited:

- R12 feature rows retained for affordable optimization, owned-fleet/carrier operations board, custody timeline, low-connectivity driver execution, explainable ETA/promise risk, no-code routing/tender policies, small-carrier portal, driver-centered workflow, dock/warehouse/transport collaboration, detention/accessorial evidence, customer self-service exception choices, multimodal continuity, network optimization, dynamic replanning, carrier intelligence, predictive ETA/exception risk, freight marketplace/shared capacity, transportation control tower, carbon/alternative-energy planning, autonomous/robotic handoff readiness, automated freight audit/dispute, digital transportation document orchestration, and foundation depth.
- RoutArr has no R12 workflow rows in the roadmap rollout maps; the existing workflow catalog remains retained full scope and does not authorize widening beyond the current R12 product pass.
- Current transportation-demand, tender, planning, visibility, capacity, yard, collaboration, claim, packet, finance, tenant-settings, and customer/driver portal slices were audited for overstated advanced capability, raw product/source labels, and implementation details shown to ordinary users.

Files changed:

- `apps/routarr-frontend/src/components/TransportationDemandsPanel.tsx`
- `apps/routarr-frontend/src/components/TransportationDemandsPanel.test.tsx`
- `docs/roadmap/products/routarr.md`

Completed R12 fixes:

- Added display helpers for source products, source references, packet types, target products, and external references in the transportation-demand workspace.
- Replaced normal-user displays of raw source/product tokens such as `ordarr`, `supplyarr:carrier:...`, document packet keys, finance target product keys, and raw actor refs with readable product and reference labels.
- Renamed the visibility panel from "Control tower events" to "Visibility events" because the R12 transportation control tower remains deferred.
- Replaced the new manual visibility event source value from `manual_control_tower` to `manual_visibility_update` so created events do not claim control-tower capability.
- Adjusted capacity and yard copy to describe StaffArr/MaintainArr references without exposing internal IDs as the primary user-facing label.

Tests run:

- `npm test -- TransportationDemandsPanel.test.tsx` from `apps/routarr-frontend` - passed 1 file / 3 tests.
- `npm run test:theme` from `apps/routarr-frontend` - passed with no theme audit violations.
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrTmsRuntimeTests|FullyQualifiedName~RoutArrTenantSettingsServiceTests" --logger "console;verbosity=minimal"` - passed 9 tests. Existing NU1510 health-check package prune warnings and Microsoft.EntityFrameworkCore.Relational 10.0.4/10.0.8 conflict warnings remain.

Remaining blockers and deferrals:

- No R12 blockers remain in the audited RoutArr current slice.
- Transportation control tower, freight marketplace/shared capacity, carbon/alternative-energy planning, autonomous/robotic handoff readiness, predictive ETA/exception risk, network optimization, dynamic replanning, automated freight audit/dispute, digital transportation document orchestration, no-code routing/tender policy simulation, small-carrier portal expansion, customer self-service exception choices, and deeper multimodal continuity remain retained R12 scope but were not started in this pass.
- Low-connectivity driver execution remains limited to the current driver portal offline queue slice; suite-grade Field Companion/mobile offline execution remains Field Companion/source-owned future work.
- StaffArr/TrainArr people and qualifications, MaintainArr asset readiness, LoadArr warehouse execution, SupplyArr carrier/vendor truth, OrdArr order promise, CustomArr customer truth, AssurArr quality decisions, RecordArr files, LedgArr accounting, Compliance Core guidance, and ReportArr analytics remain source-owned dependencies.

R12 stage result: RoutArr is clear for the R12 suite gate.
