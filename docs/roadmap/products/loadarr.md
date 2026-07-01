# LoadArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `loadarr` |
| Category | WMS / inventory |
| Entry release | R5 — Procure, receive, put away, reserve, and issue |
| Completion release | R5 — Procure, receive, put away, reserve, and issue |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Receiving, putaway, locations-as-references, item balances, stock ledger, reservations, picks, issues, transfers, counts, and discrepancies. |
| Roadmap slice | Parts/procurement/inventory loop |
| Must not violate | Replace fixture/no-op/local-success behavior before any production inventory reliance. |
| Feature rows retained | 71 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R5 | Procure, receive, put away, reserve, and issue | 36 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R5 unless they are only supporting another release gate.
- Common category baseline remains retained for R5.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## R0 trust status

- The LoadArr frontend no longer falls back to local snapshot workspace data when the API fails, and it no longer fabricates receiving or transfer list rows after a write response.
- LoadArr workspace and warehouse API groups now enforce server-side read/write authorization before returning data or accepting operational mutations.
- LoadArr no longer exposes scaffold-backed workspace summary or location-utilization surfaces as if they were authoritative. Those reads now fail truthfully with dependency/read-model unavailable responses until owner-backed data and warehouse read models exist.
- LoadArr workspace location-reference list/detail/tree surfaces no longer return scaffold-backed warehouse locations. Those reads now resolve tenant-scoped StaffArr-owned location references through service-token-backed integration lookups while keeping capacity, utilization, and other warehouse read-model data safely gated.
- LoadArr workspace site-source metadata no longer fails closed behind hardcoded ownership wiring. That metadata now resolves tenant-scoped StaffArr-owned site references through the same owner-backed integration seam used for warehouse location filtering, without exposing internal StaffArr org-unit GUIDs on the ordinary workspace surface.
- LoadArr workspace SupplyArr item-reference list no longer returns scaffold-backed part snapshots as production truth. That reference picker now resolves tenant-scoped active stocked items through an owner-backed SupplyArr service-token lookup and now projects SupplyArr-owned traceability-required truth, while granular lot-vs-serial and hazard/SDS requirements remain R0-gated until SupplyArr projects them authoritatively for warehouse write paths.
- LoadArr receiving and transfer list/detail storage now persists tenant-scoped server truth, and tenant isolation is covered by integration tests. Receiving and transfer draft creation now validates active StaffArr-owned locations plus SupplyArr-owned item references on the server, requires a client request id for retry safety, and persists durable tenant-scoped drafts while reusing the same draft on identical create retries and rejecting reused request ids with different payloads as conflicts. LoadArr receiving completion is now reopened only for saved single-line clean drafts: the server revalidates tenant-scoped StaffArr location plus SupplyArr item-reference truth, blocks inspection/discrepancy and missing-traceability cases truthfully, persists authoritative origin event/movement/balance/putaway-task rows, and returns the same warehouse completion on retry instead of double-posting inventory truth. Transfer completion actions remain truth-gated until authoritative warehouse movement and balance truth exists. Cancellation actions for those workflows are now also truth-gated because LoadArr does not yet persist authoritative cancellation audit state for the tenant, and backend tests prove the gated cancel paths do not mutate durable rows. Frontend tests now prove draft save uses owner-backed location references even when the workspace summary read model is unavailable, sends retry-safe client request ids, completes only against the saved draft route, blocks stale unsaved edits from being completed, and preserves operator input with truthful API error messages when writes fail.
- LoadArr field inbox tasks no longer synthesize receiving work from static fixture sessions or broad authenticated access. The inbox now enforces LoadArr workspace-read authority and projects only tenant-scoped persisted receiving sessions that are still actionable.
- LoadArr hold and unexplained-inventory endpoints no longer return scaffold-backed records or fake operational mutations as live warehouse truth. Those list/detail/write routes now fail closed with dependency/read-model unavailable responses, and the frontend tests verify operators keep their form input when those writes fail.
- LoadArr count and adjustment endpoints no longer expose scaffold-backed count sessions, approvals, or adjustments as production inventory truth. Those list/detail/write routes now fail closed with dependency/read-model unavailable responses, and the frontend tests verify count and adjustment forms preserve operator input when writes fail.
- LoadArr truck stock and kit endpoints no longer expose scaffold-backed mobile-stock, kit composition, or kit workflow mutations as production inventory truth. Those list/detail/write routes now fail closed with dependency/read-model unavailable responses, and the frontend tests verify blocked actions stay disabled while failed writes preserve operator input.
- LoadArr visible staging navigation no longer routes into the retained truck-stock workflow. The `/work/staging` route now stays behind the authoritative staging-assignment gate, while truck stock remains separately reachable under its own honest work route instead of masquerading as staging.
- LoadArr stock-ledger and warehouse-history record surfaces no longer expose scaffold-backed ledger, receiving, movement, count, or adjustment records as operational warehouse truth. Those API routes now fail closed with dependency/read-model unavailable responses, and the dedicated stock-ledger, receiving-history, count-history, and adjustment-history UI routes stay safely gated until authoritative warehouse history exists for the tenant.
- LoadArr route-surface dashboard, queue, exception, supply-coordination, and setup endpoints no longer expose scaffold-backed operational or administrative records as production truth. Those API routes now fail closed with dependency/read-model unavailable responses, and the paired dashboard, expected-receipts, dock-schedule, staging, and shipping UI routes stay safely gated instead of synthesizing local record queues.
- LoadArr supply coordination navigation no longer routes `Purchase Order Receipts` into the work-side expected-receipts surface or `Reorder Signals` into a generic supply summary. Those destinations now show their own truthful blocked states, and the legacy `/work/issues` alias no longer lands on warehouse exceptions.
- LoadArr setup navigation no longer routes `Location Rules`, `Item / Part References`, `Inventory Policies`, or `Devices & Labels` into tenant settings or inventory balance rollups. Those setup destinations now stay explicitly blocked behind the authoritative setup route-surface gate instead of reusing unrelated surfaces.
- LoadArr integration endpoints no longer return empty fixture collections or echo inbound request payloads as tenant warehouse truth. Those read/write routes now enforce server-side integration permissions and fail closed with `dependency_unavailable` until authoritative tenant-scoped integration synchronization exists.
- LoadArr frontend admin integrations no longer reuse Shipping / Loadout handoff UI or the anonymous `/handoffs` alias. The `/admin/integrations` route now probes the real integration read surface and shows truthful permission-denied or synchronization-unavailable states instead of recycled warehouse workflow content.
- LoadArr workspace site-source metadata and admin permission catalog surfaces no longer treat hardcoded ownership wiring or broad authenticated access as trustworthy ordinary-product behavior. Site-source metadata now resolves tenant-scoped StaffArr-owned site references through a server-side permissioned workspace read, and the permission catalog requires LoadArr read-level administrative authority on the server.
- LoadArr session bootstrap no longer exposes retired product-access grant wording as live runtime contract. The API now returns neutral suite-launch context only, while the frontend remains tolerant of older payloads during transition.
- LoadArr handoff redemption no longer treats `launchableProductKeys` as an ordinary-product access gate. Valid LoadArr-target handoffs now redeem based on active tenant context and target-product match, preserving the suite-wide launch model while keeping downstream product permissions server-side.
- Remaining R0 blocker: transfer workflows still need authoritative warehouse movement and balance truth before completion paths can reopen, and receiving drafts that require inspection, discrepancy review, or richer traceability semantics still need authoritative quality/hold and owner-projected lot-vs-serial truth before those completion variants can reopen. Hold, unexplained, count, adjustment, truck stock, and kit workflows still need the same owner-backed reference synchronization plus authoritative balance/read-model support before their operational write paths can reopen. StaffArr-backed site/location reads and SupplyArr-backed active item-reference reads are now authoritative on the workspace reference surfaces, including owner-backed generic traceability-required signaling, but location-utilization/read-model support and richer write-path validation, including hazard/SDS truth, still need authoritative backend support before this product can advance from R0 to R1.

### R0 pass update — 2026-06-27

Status: complete for the current LoadArr auth/session pass, with the remaining authoritative warehouse movement/balance blockers above still deferred.

Files touched in this pass:

- `apps/loadarr-api/LoadArr.Api/Endpoints/AuthEndpoints.cs`
- `apps/loadarr-api/LoadArr.Api/Services/HandoffAuthService.cs`
- `apps/loadarr-api/LoadArr.Api/Services/LoadArrSuiteLaunchCatalog.cs`
- `apps/loadarr-api/LoadArr.Api/Settings/LoadArrAuthorizationService.cs`
- `tests/STLCompliance.LoadArr.Auth.Tests/LoadArrAuthEndpointsTests.cs`
- `tests/STLCompliance.LoadArr.Auth.Tests/LoadArrTenantSettingsTests.cs`

Completed R0 fixes:

- Handoff redemption and session bootstrap now use a fixed ordinary-suite launch catalog instead of claim-carried or NexArr-returned `launchableProductKeys`.
- LoadArr-target handoff redemption remains valid for active tenant context even when the redeemed launch context did not include LoadArr.
- Compliance Core remains excluded from ordinary tenant launch availability.
- Internal authorization helper language now uses launch-context wording instead of product entitlement wording.

Tests run:

- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --filter "FullyQualifiedName~LoadArrAuthEndpointsTests|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrTenantSettingsTests.Session_bootstrap_allows_warehouse_manager_after_non_loadarr_launch_context|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrTenantSettingsTests.Tenant_settings_get_allows_warehouse_manager_after_non_loadarr_launch_context|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrTenantSettingsTests.Tenant_settings_get_seeds_defaults_and_audit_without_internal_ids" --logger "console;verbosity=minimal"` — passed 10 tests.
- `npm test -- client.test.ts mutationMessages.test.ts App.test.tsx` from `apps/loadarr-frontend` — passed 3 files / 51 tests.
- Current repo-state rerun: `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~LoadArrAuthEndpointsTests|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrTenantSettingsTests.Session_bootstrap_allows_warehouse_manager_after_non_loadarr_launch_context|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrTenantSettingsTests.Tenant_settings_get_allows_warehouse_manager_after_non_loadarr_launch_context|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrTenantSettingsTests.Tenant_settings_get_seeds_defaults_and_audit_without_internal_ids" --logger "console;verbosity=minimal"` — passed 10 tests in 12s.
- Current repo-state rerun: `npm test -- --run client.test.ts mutationMessages.test.ts App.test.tsx` from `apps/loadarr-frontend` — passed 3 files / 51 tests in 24.56s.

## R1 Foundation spine pass

Status: Not applicable. LoadArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R5.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no LoadArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no LoadArr rows for `R1`.
- LoadArr's product FEATURESET and WORKFLOWS remain retained full scope, including the R0 movement/balance blockers, but they do not authorize starting the R5 warehouse inventory loop during the R1 suite stage.

Files touched:

- `docs/roadmap/products/loadarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no LoadArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. The deferred R0 authoritative warehouse movement, balance, traceability, and cancellation-audit blockers remain active for the later LoadArr R5 stage.

R1 stage result: LoadArr is clear for the R1 suite gate as not applicable.

## R5 Procure, receive, put away, reserve, and issue pass

Status: Clear for the implemented LoadArr-owned clean receiving and traceability slice, with deferred blockers for the broader warehouse execution workflows that still require additional authoritative transaction/read-model work.

R5 scope audited:

- `docs/roadmap/releases/r5-procure-receive-put-away-reserve-and-issue.md`
- `docs/roadmap/reference/feature-rollout-map.csv` LoadArr rows for `R5`
- `docs/roadmap/reference/workflow-rollout-map.csv` LoadArr rows for `R5`
- `docs/products/loadarr/FEATURESET.md`
- `docs/products/loadarr/WORKFLOWS.md`

Completed R5 fixes:

- LoadArr now exposes tenant-scoped durable inventory balances from completed receiving through `/api/v1/workspace/inventory` instead of leaving the committed balance invisible behind a blanket unavailable state.
- LoadArr now exposes tenant-scoped durable warehouse tasks from completed receiving through `/api/v1/workspace/tasks`, including the generated putaway task for the clean receive path.
- LoadArr now exposes tenant-scoped durable receiving history, movement history, and stock-ledger records from persisted receiving sessions and inventory movements through the records surface.
- The R5 receiving completion test now proves a clean single-line receipt commits exactly one origin event, movement, balance, and putaway task, returns the same committed records on retry, exposes those records through ordinary authorized reads, and keeps another tenant isolated.
- Empty durable records surfaces now return successful empty lists instead of fake fixture records or misleading dependency failures; count and adjustment history remain unavailable until their authoritative workflows exist.

Deferred R5 blockers:

- Transfer completion remains gated because LoadArr does not yet perform the atomic source/destination balance movement required for production warehouse transfer truth.
- Reservation, allocation, issue, pick, pack, staging, shipping, replenishment, truck stock, kit, count, adjustment, hold/quarantine, unexplained inventory, returns, and reverse-logistics write paths remain gated until their authoritative movement, balance, audit, and read-model support exists.
- Receiving variants that require inspection, discrepancy handling, AssurArr quality disposition, richer lot/serial/SDS semantics, or RecordArr durable evidence retention remain gated rather than silently completing.
- Expected receipt/ASN, dock appointment, route surface dashboard/queues, setup read models, and integration synchronization remain gated until owner-backed synchronization is implemented.
- RecordArr durable file retention remains a suite blocker for evidence packages; LoadArr references evidence summaries only in the current R5 slice.

Files touched:

- `apps/loadarr-api/LoadArr.Api/Services/LoadArrOperationalWorkflowStore.cs`
- `apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs`
- `apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs`
- `tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs`
- `docs/roadmap/products/loadarr.md`
- `docs/roadmap/releases/r5-procure-receive-put-away-reserve-and-issue.md`

Tests run:

- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrIntegrationAuthTests.Receiving_complete_persists_authoritative_warehouse_truth_and_retries_idempotently" --logger "console;verbosity=minimal"` - passed 1 test.
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - passed 111 tests.
- `npm test` from `apps/loadarr-frontend` - passed 7 files / 60 tests.
- Current repo-state rerun: `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrIntegrationAuthTests.Receiving_complete_persists_authoritative_warehouse_truth_and_retries_idempotently|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrIntegrationAuthTests.Receiving_list_and_detail_return_persisted_sessions_and_isolate_tenant|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrIntegrationAuthTests.Receiving_create_persists_draft_with_authoritative_references|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrIntegrationAuthTests.Receiving_create_reuses_existing_draft_when_client_request_id_is_retried|FullyQualifiedName=STLCompliance.LoadArr.Auth.Tests.LoadArrIntegrationAuthTests.Receiving_create_conflicts_when_client_request_id_is_reused_for_different_payload" --logger "console;verbosity=minimal"` - passed 5 tests in 5s.
- Current repo-state rerun: `npm test -- --run client.test.ts mutationMessages.test.ts App.test.tsx` from `apps/loadarr-frontend` - passed 3 files / 51 tests in 5.52s.

R5 stage result: LoadArr is clear for the R5 suite gate with the deferred blockers above documented. The suite may advance to R6 only after the R5 release summary records both SupplyArr and LoadArr results.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for the current LoadArr R12 trust pass, with advanced warehouse execution depth still deferred behind the documented authoritative transaction, read-model, offline-sync, and integration blockers.

R12 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` LoadArr rows for `R12`
- `docs/roadmap/reference/workflow-rollout-map.csv` LoadArr rows for `R12`
- `docs/products/loadarr/FEATURESET.md`
- `docs/products/loadarr/WORKFLOWS.md`
- Current LoadArr tenant settings, integration, field inbox, workspace, route-surface, and frontend settings slices

Completed R12 fixes:

- LoadArr tenant settings no longer present retained offline mobile execution as a live operational promise. The persisted settings contract remains unchanged, but the user-facing mobile/scanner metadata now describes offline work as readiness policy until authoritative offline sync and conflict handling are implemented.
- Offline risk warnings now state that authoritative offline execution remains gated until sync routes through owning-product validation and preserves inventory conflicts, rather than implying the current settings toggle enables production offline task execution.
- Backend and frontend tests now cover the revised offline-readiness labels and warnings while preserving tenant-scoped settings access, audit behavior, warning acknowledgement, and frontend draft validation.

Deferred R12 blockers:

- `LO-WF-014` offline mobile warehouse task and sync remains deferred until Field Companion/offline queue support and LoadArr authoritative sync can validate through owning-product references, preserve inventory conflicts, and avoid silent last-write-wins behavior for ledger-affecting fields.
- `LO-WF-015` spreadsheet/legacy warehouse cutover remains deferred until LoadArr has durable import/cutover tooling, opening-balance ledger transactions, reconciliation reporting, and rollback/audit support.
- Scan-first UX, small warehouse mode, custody explanations, unexplained-inventory investigation, shortage resolution, human-readable inventory status, exception collaboration, shared warehouse support, 3PL billing contribution, and regulated/cold-chain controls remain deferred beyond the clean receiving slice until the broader warehouse transaction engine and read models exist.
- Voice/wearable workflows, advanced WES/automation orchestration, computer vision, dynamic slotting/digital twin, labor planning, waveless orchestration, robotics/AMR integrations, inventory risk prediction, and yard-to-warehouse control tower remain retained R12 scope but are not started in this pass.
- RecordArr durable evidence packets, AssurArr quality disposition, SupplyArr procurement/shortage references, Field Companion offline capture, and ReportArr advanced reporting remain suite dependencies rather than LoadArr-owned source truth.

Files touched:

- `apps/loadarr-api/LoadArr.Api/Settings/LoadArrTenantSettingsDefaults.cs`
- `apps/loadarr-api/LoadArr.Api/Settings/LoadArrTenantSettingsValidator.cs`
- `apps/loadarr-frontend/src/components/TenantSettingsPanel.tsx`
- `apps/loadarr-frontend/src/components/TenantSettingsPanel.test.ts`
- `tests/STLCompliance.LoadArr.Auth.Tests/LoadArrTenantSettingsTests.cs`
- `docs/roadmap/products/loadarr.md`

Tests run:

- `rg -n "Allow offline task execution|Offline sync conflict policy|Controls barcode scans, manual entry, camera/external scanners, offline execution|Offline execution must not silently resolve inventory conflicts|Offline execution must sync through" apps/loadarr-api apps/loadarr-frontend tests/STLCompliance.LoadArr.Auth.Tests -S` - no matches.
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --filter "FullyQualifiedName~LoadArrTenantSettingsTests|FullyQualifiedName~LoadArrFieldInboxTests|FullyQualifiedName~LoadArrIntegrationAuthTests" --logger "console;verbosity=minimal"` - passed 104 tests.
- `npm test -- --run src/components/TenantSettingsPanel.test.ts src/api/client.test.ts src/App.test.tsx` from `apps/loadarr-frontend` - passed 3 files / 52 tests.

R12 stage result: LoadArr is clear for the R12 suite gate with the deferred blockers above documented. The next R12 product pass is AssurArr.

## Source docs

- [Feature catalog](../../products/loadarr/FEATURESET.md)
- [Workflow catalog](../../products/loadarr/WORKFLOWS.md)
- [Product manifest](../../products/loadarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
