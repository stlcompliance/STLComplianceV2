# OrdArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `ordarr` |
| Category | OMS |
| Entry release | R7B — Order/request orchestration baseline |
| Completion release | R7B — Order/request orchestration baseline |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Orders, requests, order lifecycle, triage, handoffs, exception coordination, completion packets, and bill-ready intent. |
| Roadmap slice | Order/request orchestration after customer master baseline |
| Must not violate | Explain why work is happening while execution products own how work is performed. |
| Feature rows retained | 66 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R7B | Order/request orchestration baseline | 31 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R7B unless they are only supporting another release gate.
- Common category baseline remains retained for R7B.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/ordarr/FEATURESET.md)
- [Workflow catalog](../../products/ordarr/WORKFLOWS.md)
- [Product manifest](../../products/ordarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: complete for the current OrdArr auth/session and persistence-configuration pass, with deferred scaffold-store blockers.

Files changed:

- `apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs`
- `apps/ordarr-api/OrdArr.Api/Endpoints/AuthEndpoints.cs`
- `apps/ordarr-api/OrdArr.Api/OrdArrServiceRegistration.cs`
- `apps/ordarr-api/OrdArr.Api/Services/HandoffAuthService.cs`
- `apps/ordarr-api/OrdArr.Api/Services/OrdArrSuiteLaunchCatalog.cs`
- `apps/ordarr-frontend/src/App.test.tsx`
- `apps/ordarr-frontend/src/api/client.ts`
- `tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs`

Completed R0 fixes:

- OrdArr no longer falls back to EF InMemory outside Testing. Missing `DATABASE_URL` or `ConnectionStrings:Database` now fails startup instead of creating production in-memory order truth.
- Handoff redemption and session bootstrap now return a fixed ordinary-suite launch catalog rather than trusting NexArr-returned or claim-carried launchable product keys.
- Removed the handoff-time product availability gate so active tenant context plus OrdArr target-product match controls launch, while record/action permissions stay server-side.
- Session bootstrap no longer emits the retired `hasOrdArrAccess` product-access flag.
- The frontend session type no longer carries `hasOrdArrAccess`; legacy payloads are tolerated and stripped during normalization.
- Compliance Core remains excluded from ordinary tenant launch availability.

Tests run:

- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --filter "FullyQualifiedName~OrdArrAuthEndpointsTests|FullyQualifiedName~OrdArrStoreTests" --logger "console;verbosity=minimal"` — passed 13 tests. The run still reports existing nullability warnings in the scaffold `OrdArrStore`.
- `npm test -- App.test.tsx sessionStorage.test.ts` from `apps/ordarr-frontend` — passed 2 files / 10 tests.

Deferred R0 blockers:

- `OrdArrStore` remains a singleton, process-local operational order store. It is still not restart-safe and cannot provide production-grade tenant durability, concurrency, outbox delivery, or audit history. OrdArr is not production-trust-clear until this store is replaced with tenant-scoped durable persistence, migrations, concurrency controls, outbox/reconciliation, and audit history.

## R1 Foundation spine pass

Status: Not applicable. OrdArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R7B.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no OrdArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no OrdArr rows for `R1`.
- OrdArr's product FEATURESET and WORKFLOWS remain retained full scope, including the R0 process-local store blocker, but they do not authorize starting the R7B order/request orchestration baseline during the R1 suite stage.

Files touched:

- `docs/roadmap/products/ordarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no OrdArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. The deferred R0 durable order-store blocker remains active for the later OrdArr R7B stage.

R1 stage result: OrdArr is clear for the R1 suite gate as not applicable.

## R7B Order/request orchestration baseline pass

Status: complete for the current OrdArr order/request orchestration baseline.

Files changed:

- `apps/ordarr-api/OrdArr.Api/Data/OrdArrDbContext.cs`
- `apps/ordarr-api/OrdArr.Api/Data/OrdArrDesignTimeDbContextFactory.cs`
- `apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs`
- `apps/ordarr-api/OrdArr.Api/Migrations/20260627162841_AddOrdArrDurableOrderStore.cs`
- `apps/ordarr-api/OrdArr.Api/Migrations/20260627162841_AddOrdArrDurableOrderStore.Designer.cs`
- `apps/ordarr-api/OrdArr.Api/Migrations/OrdArrDbContextModelSnapshot.cs`
- `apps/ordarr-api/OrdArr.Api/OrdArrServiceRegistration.cs`
- `tests/STLCompliance.OrdArr.Auth.Tests/OrdArrStoreTests.cs`
- `tests/STLCompliance.MaintainArr.Auth.Tests/OrdArrCustomArrHandoffTests.cs`
- `docs/roadmap/products/ordarr.md`
- `docs/roadmap/releases/r7b-order-request-orchestration-baseline.md`

Completed R7B fixes and verification:

- Replaced the production blocker where OrdArr operational order truth lived only in a singleton, process-local list.
- Added durable EF-backed order snapshot and idempotency tables for tenant-scoped order/request records, lifecycle, holds, lines, handoffs, returns, completion packets, and bill-ready intent as preserved API payloads.
- Added the first OrdArr EF migration and design-time DbContext factory so Render startup migrations can create the durable order store.
- Changed `OrdArrStore` registration from singleton to scoped so each request uses the EF-backed store with database-loaded state.
- Preserved OrdArr's ownership boundary: OrdArr stores customer and execution references, but does not copy CustomArr customer truth, LoadArr warehouse truth, RoutArr transport execution, SupplyArr procurement truth, AssurArr quality decisions, RecordArr files, or LedgArr financial execution.
- Added restart/recreation coverage proving orders and idempotency survive a fresh store instance.
- Updated the existing OrdArr/CustomArr handoff smoke to use EF-backed OrdArr and CustomArr contexts.

Tests run:

- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.OrdArr.Auth.Tests.OrdArrStoreTests.Orders_and_idempotency_survive_store_recreation" --logger "console;verbosity=minimal"` — passed 1 test.
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` — passed 14 tests.
- `npm test` from `apps/ordarr-frontend` — passed 2 files / 10 tests.
- `npm run test:theme` from `apps/ordarr-frontend` — passed with no theme audit violations.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~OrdArrCustomArrHandoffTests" --logger "console;verbosity=minimal"` — passed 2 tests.

Known warnings:

- The .NET test runs still emit existing NU1510 health-check package prune warnings and Microsoft.EntityFrameworkCore.Relational 10.0.4/10.0.8 conflict warnings.
- The MaintainArr smoke test project still emits an existing nullable warning in `MaintainArrWorkOrderTests.cs`.

Remaining blockers and deferrals:

- No R7B blockers remain for the audited order/request orchestration baseline.
- Durable outbox delivery/reconciliation is still represented only by stored order events and idempotency records; a background reconciliation worker remains future foundation depth.
- OR-WF-003 order promising, OR-WF-009 downstream change compensation, OR-WF-011 partial/backorder decision support, OR-WF-014 exception control tower, and OR-WF-015 full durable event outbox/reconciliation remain retained roadmap scope for later stages.
- R8 transportation execution remains RoutArr-owned and was not pulled into OrdArr. R12 portal, EDI/marketplace, import/export, advanced reporting, AI, and category-depth work remains deferred.

R7B stage result: OrdArr is clear for the R7B suite gate.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: complete for the current OrdArr R12 trust and UI-scope pass, with advanced expansion scope retained as deferred roadmap work.

R12 scope audited:

- R12 feature rows retained for advanced order management, explainable promise dates, self-service customer changes, exception-first operations, low-cost EDI/API/portal intake, partial fulfillment decision support, distributed order management, intelligent promising, fulfillment optimization, fraud/anomaly review, subscriptions, marketplace/partner order hub, order control tower, document packet generation, mass order simulation, customer-specific policies, and category-depth foundation.
- R12 workflow rows retained for `OR-WF-014` order exception control tower and recovery and `OR-WF-015` durable order event outbox and reconciliation.
- Current OrdArr frontend, API client, EF-backed store, endpoint tests, and UI tests were audited for overstated R12 behavior, raw product-key display, internal endpoint display, and scaffold-only advanced capability exposure.

Files changed:

- `apps/ordarr-frontend/src/App.tsx`
- `apps/ordarr-frontend/src/App.test.tsx`
- `docs/roadmap/products/ordarr.md`

Completed R12 fixes:

- Replaced normal-user displays of raw product keys in customer references, timeline entries, order lines, holds, and handoffs with product display labels while preserving the API contract and stored source references.
- Replaced free-text target/owner product key inputs with display-name selects backed by the existing product-key values, reducing operator mistakes without creating new cross-product ownership.
- Removed local API/frontend port and endpoint-coordinate copy from the ordinary settings page. Settings now describes runtime readiness, suite-shell launch, authenticated session state, tenant context, and ownership boundaries.
- Added frontend coverage proving downstream handoffs render product names instead of raw keys and settings no longer exposes local port details.

Tests run:

- `npm test -- App.test.tsx sessionStorage.test.ts` from `apps/ordarr-frontend` - passed 2 files / 11 tests.
- `npm run test:theme` from `apps/ordarr-frontend` - passed with no theme audit violations.
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - passed 14 tests. The run still reports existing NU1510 health-check package prune warnings and Microsoft.EntityFrameworkCore.Relational 10.0.4/10.0.8 conflict warnings.

Remaining blockers and deferrals:

- No R12 blockers remain in the audited OrdArr current slice.
- `OR-WF-014` remains deferred until normalized cross-product exception feeds, ownership/action queues, recovery options, and outcome/cause evidence exist across the source-owning products.
- `OR-WF-015` remains deferred until atomic outbox publishing, retry/dead-letter handling, downstream acknowledgement, drift detection, replay tooling, and repair workflows exist.
- Customer self-service portals, EDI/marketplace/partner intake, subscriptions, intelligent promising, fulfillment optimization, fraud/anomaly review, mass order simulation, automated packet generation, customer-specific orchestration policies, and advanced category-depth functions remain retained R12 scope but were not started in this pass.
- CustomArr customer truth, LoadArr inventory/warehouse truth, RoutArr transportation execution, MaintainArr service execution, SupplyArr procurement truth, AssurArr quality decisions, RecordArr retained files, LedgArr accounting execution, Compliance Core guidance, ReportArr analytics, and Field Companion mobile capture remain source-owned dependencies.

R12 stage result: OrdArr is clear for the R12 suite gate.
