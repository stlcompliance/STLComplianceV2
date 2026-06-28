# SupplyArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `supplyarr` |
| Category | SRM / procurement |
| Entry release | R5 — Procure, receive, put away, reserve, and issue |
| Completion release | R5 — Procure, receive, put away, reserve, and issue |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Suppliers, vendors, procurement expectations, purchase requests/orders, sourcing, pricing, and lead-time context. |
| Roadmap slice | Parts/procurement/inventory loop |
| Must not violate | Own commercial/procurement truth while LoadArr owns physical inventory and CustomArr owns customers. |
| Feature rows retained | 72 |
| Workflow rows retained | 16 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R5 | Procure, receive, put away, reserve, and issue | 37 | 16 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R5 unless they are only supporting another release gate.
- Common category baseline remains retained for R5.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/supplyarr/FEATURESET.md)
- [Workflow catalog](../../products/supplyarr/WORKFLOWS.md)
- [Product manifest](../../products/supplyarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: complete for the current SupplyArr auth/session slice, with deferred ownership blockers.

Files changed:

- `apps/supplyarr-api/SupplyArr.Api/Contracts/AuthContracts.cs`
- `apps/supplyarr-api/SupplyArr.Api/Endpoints/CoverageAliasEndpoints.cs`
- `apps/supplyarr-api/SupplyArr.Api/Services/HandoffAuthService.cs`
- `apps/supplyarr-api/SupplyArr.Api/Services/MeService.cs`
- `apps/supplyarr-api/SupplyArr.Api/Services/SupplyArrAuthorizationService.cs`
- `apps/supplyarr-api/SupplyArr.Api/Services/SupplyArrSuiteLaunchCatalog.cs`
- `apps/supplyarr-frontend/src/api/types.ts`
- `apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx`
- `tests/STLCompliance.SupplyArr.Auth.Tests/SupplyArrHandoffApiTests.cs`

Completed R0 fixes:

- Handoff redemption and `/api/me`/`/api/session` now return a fixed ordinary-suite launch catalog for SupplyArr instead of trusting mutable redeemed launchable product keys.
- Removed the user-facing `HasSupplyArrAccess`/`hasSupplyArrAccess` contract field so SupplyArr availability is not represented as variable product entitlement.
- Kept Compliance Core out of ordinary tenant launch availability while preserving the rest of the suite catalog.
- Renamed the misleading SupplyArr entitlement gate to launch-context language while preserving server-side action permission checks.
- Allowed platform-admin tenant context through the basic supplier/part read/manage gates, matching the existing SupplyArr R0 workflow tests and the product's other administrative gates.
- Updated the coverage/admin overview response to use the fixed ordinary launch catalog instead of claim-carried launch keys.

Tests run:

- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrHandoffApiTests.Handoff_redeem_happy_path_returns_session_and_me_works|FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrHandoffApiTests.Handoff_redeem_nexarr_alias_happy_path_returns_session|FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrHandoffApiTests.V1_handoff_session_and_me_aliases_work|FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrHandoffApiTests.V1_bootstrap_approvals_stock_transactions_and_cycle_counts_aliases_work|FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrHandoffApiTests.Me_allows_users_after_non_supplyarr_launch_context" --logger "console;verbosity=minimal"` — passed 5 tests. A prior full `SupplyArrHandoffApiTests` run timed out and left a stale `testhost`; that process was stopped before rerunning the focused tests.
- `npm test -- client.test.ts ProductWorkspaceLayout.test.tsx sessionStorage.test.ts` from `apps/supplyarr-frontend` — passed 3 files / 47 tests.

Deferred R0 blockers:

- SupplyArr still contains WMS-style location, stock, reservation, ledger, receiving execution, and outbound shipment surfaces that the product docs identify as LoadArr-owned boundary debt. This pass did not migrate or remove those capabilities because doing so would exceed the current auth/session slice and risks feature deletion. SupplyArr is not production-trust-clear for physical inventory ownership until those records are migrated or replaced by LoadArr projections/API/event references.

## R1 Foundation spine pass

Status: Not applicable. SupplyArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R5.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no SupplyArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no SupplyArr rows for `R1`.
- SupplyArr's product FEATURESET and WORKFLOWS remain retained full scope, including the LoadArr-owned inventory boundary debt noted in R0, but they do not authorize starting the R5 procure/receive/put-away/reserve/issue loop during the R1 suite stage.

Files touched:

- `docs/roadmap/products/supplyarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no SupplyArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. The deferred R0 physical-inventory ownership blocker remains active for the later SupplyArr/LoadArr product-stage work, but it is not an R1-stage blocker because SupplyArr has no R1 scope.

R1 stage result: SupplyArr is clear for the R1 suite gate as not applicable.

## R5 Procure, receive, put away, reserve, and issue pass

Status: Clear for the SupplyArr-owned commercial/procurement R5 slice with explicit LoadArr ownership deferral for physical inventory execution.

R5 scope audited:

- SupplyArr has 37 R5 feature rows and 16 R5 workflow rows in the roadmap rollout maps.
- Durable/current commercial workflows audited and verified: supplier onboarding, restrictions, supplier incidents, approval authority, demand intake/processing, RFQ/quote, purchase requests, purchase orders, vendor order portal/status, procurement exceptions and escalation, emergency purchase, returns/warranty, contracts/imports, price/lead-time/availability snapshots, coordination workers, notifications, reports, audit history, supplier readiness, reference integration, and compliance fact publication.
- SupplyArr remains the supplier, sourcing, purchase request/order, vendor acknowledgement, procurement exception, supplier performance, and commercial recovery authority. LoadArr remains physical inventory, receiving execution, putaway, stock ledger, reservation, pick, issue, transfer, count, and warehouse movement authority.

Completed blockers and shared fixes:

- Mapped the existing SupplyArr report index endpoint so `/api/v1/reports` is auth-gated and advertises the v1 report families instead of returning an anonymous 404.
- Mapped the existing stock reservation endpoint group so legacy reservation tests hit server authorization and movement safety rather than route fallthrough.
- Tightened SupplyArr reference integration catalog/schema/search/quick-create endpoints so platform admins without a SupplyArr tenant role do not bypass ordinary tenant action permission expectations on normal reference APIs.
- Repaired stock-reservation and integration-event test fixtures to include the active StaffArr site reference required by the movement safety rule before stock moves.

Files touched:

- `apps/supplyarr-api/SupplyArr.Api/Program.cs`
- `apps/supplyarr-api/SupplyArr.Api/Endpoints/ReferenceIntegrationEndpoints.cs`
- `tests/STLCompliance.SupplyArr.Auth.Tests/SupplyArrStockReservationTests.cs`
- `tests/STLCompliance.SupplyArr.Auth.Tests/SupplyArrIntegrationEventTests.cs`
- `docs/roadmap/products/supplyarr.md`

Tests run:

- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --filter "FullyQualifiedName~SupplyArrSupplierOnboardingTests|FullyQualifiedName~SupplyArrVendorRestrictionTests|FullyQualifiedName~SupplyArrStaffarrProcurementApprovalAuthorityTests|FullyQualifiedName~SupplyArrDemandProcessingWorkerTests|FullyQualifiedName~SupplyArrRfqTests|FullyQualifiedName~SupplyArrProcurementExceptionTests|FullyQualifiedName~SupplyArrProcurementExceptionEscalationWorkerTests|FullyQualifiedName~SupplyArrVendorEmailInboxTests|FullyQualifiedName~SupplyArrVendorCatalogApiTests" --logger "console;verbosity=minimal"` - passed, 39 tests.
- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrPurchasingReportTests.Report_index_requires_auth_and_advertises_v1_report_paths" --logger "console;verbosity=minimal"` - passed, 1 test.
- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~SupplyArrWarrantyClaimTests|FullyQualifiedName~SupplyArrVendorReportTests|FullyQualifiedName~SupplyArrPurchasingReportTests|FullyQualifiedName~SupplyArrPartsInventoryReportTests|FullyQualifiedName~SupplyArrComplianceReportTests|FullyQualifiedName~SupplyArrAuditHistoryTests" --logger "console;verbosity=minimal"` - passed, 28 tests.
- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~SupplyArrSupplyReadinessDashboardTests|FullyQualifiedName~SupplyArrSupplyReadinessCheckTests|FullyQualifiedName~SupplyArrNotificationTests|FullyQualifiedName~SupplyArrPriceSnapshotWorkerTests|FullyQualifiedName~SupplyArrLeadTimeSnapshotWorkerTests|FullyQualifiedName~SupplyArrAvailabilitySnapshotWorkerTests|FullyQualifiedName~SupplyArrProcurementCoordinationWorkerTests" --logger "console;verbosity=minimal"` - passed, 33 tests.
- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --filter "FullyQualifiedName~SupplyArrStockReservationTests|FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrIntegrationEventTests.Product_qualified_outbound_events_are_enqueued_for_core_supply_workflows|FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrReferenceIntegrationAuthTests.Reference_types_catalog_rejects_platform_admin_without_supplyarr_role|FullyQualifiedName=STLCompliance.SupplyArr.Auth.Tests.SupplyArrReferenceIntegrationAuthTests.Party_quick_create_schema_disables_platform_admin_without_supplyarr_role" --logger "console;verbosity=minimal"` - passed, 6 tests.
- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~SupplyArrStockReservationTests|FullyQualifiedName~SupplyArrReorderEvaluationWorkerTests|FullyQualifiedName~SupplyArrReferenceIntegrationAuthTests|FullyQualifiedName~SupplyArrIntegrationEventTests|FullyQualifiedName~SupplyArrComplianceCoreFactPublishingTests|FullyQualifiedName~SupplyArrEmergencyPurchaseTests|FullyQualifiedName~SupplyArrLoadTestJourneySeedTests|FullyQualifiedName~SupplyArrForgivingSearchTests|FullyQualifiedName~ProcurementNotificationRulesTests|FullyQualifiedName~ProcurementExceptionRulesTests|FullyQualifiedName~ProcurementExceptionEscalationRulesTests|FullyQualifiedName~ProcurementCoordinationRulesTests|FullyQualifiedName~DemandProcessingRulesTests|FullyQualifiedName~ApprovalReminderRulesTests|FullyQualifiedName~SupplyArrApprovalReminderWorkerTests" --logger "console;verbosity=minimal"` - passed, 65 tests.
- `npm test` from `apps/supplyarr-frontend` - passed, 54 files / 151 tests.

Additional verification:

- A combined reports/workers/support cluster exceeded the local command timeout. The stale testhost was stopped and the same coverage was rerun in smaller passing clusters.
- The initial reports/audit cluster exposed the unmapped report index. The exact failing test and the full cluster passed after the route mapping fix.
- The initial support/boundary cluster exposed unmapped stock reservation routes, missing StaffArr site fixture data, and a platform-admin reference integration bypass. Exact failing tests and the full cluster passed after the fixes.

Deferred blockers:

- `SU-CUR-016` legacy WMS tables remain boundary debt. SupplyArr still contains location, stock, reservation, ledger, and outbound-shipment records because removing them during this product pass would delete represented workflows before LoadArr migration/projection is complete.
- `SU-WF-008` receipt outcome and procurement exception remains partial for the end-to-end LoadArr physical receipt handoff. SupplyArr commercial receipt/exception context is verified; receiving execution, putaway, stock ledger, and physical disposition belong to the upcoming LoadArr R5 pass.
- `SU-WF-011` supplier performance review, `SU-WF-012` supplier corrective action handoff, and `SU-WF-013` contract lifecycle remain partial/retained category-depth workflows. Existing durable records and reports are verified, but full SCAR ownership belongs to AssurArr and full contract/document-control depth remains retained scope.
- RecordArr durable file retention and AssurArr quality disposition are referenced but not completed in SupplyArr; those products remain the appropriate owners for retained documents and quality/CAPA decisions.

R5 stage result: SupplyArr is clear for the R5 product gate with the deferred blockers above. Continue R5 with LoadArr; do not advance to R6 until LoadArr clears R5 and the R5 suite summary is updated.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for R12 with explicit deferrals for advanced optimization, supplier network, contract intelligence, third-party risk monitoring, multi-tier mapping, dynamic discounting, and control-tower depth.

R12 scope audited:

- SupplyArr has 35 R12 feature rows and no R12 workflow rows in the roadmap rollout maps.
- R12-adjacent durable/current slices audited: supplier/vendor portal flows, vendor catalog API sync, procurement exception workbench, demand processing and cross-product demand status, part substitutions, supply readiness dashboard/checks, supplier restrictions/incidents, vendor reports, purchasing/compliance reports, price/lead-time/availability snapshots, import center, RecordArr vendor-order document handoff, and Compliance Core vendor-use/fact publication handoffs.
- SupplyArr remains the supplier, sourcing, purchase request/order, vendor acknowledgement, procurement exception, supplier performance, and commercial recovery authority. LoadArr remains physical inventory and movement authority, AssurArr remains quality/SCAR authority, RecordArr remains file/document lifecycle authority, ReportArr remains suite reporting authority, and Compliance Core remains regulatory meaning.

Completed blockers:

- Removed normal-user exposure of raw tenant GUID and tenant role key from the SupplyArr supplier portal operations section.
- Removed raw tenant role key display from the SupplyArr workspace shell subtitle by adding a small display-label helper. Server-side action permissions and session/auth fields remain unchanged.

Files touched:

- `apps/supplyarr-frontend/src/lib/displayLabels.ts`
- `apps/supplyarr-frontend/src/workspace/WorkspaceShell.tsx`
- `apps/supplyarr-frontend/src/workspace/sections/SupplierPortalSection.tsx`
- `docs/roadmap/products/supplyarr.md`

Tests run:

- `dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj --filter "FullyQualifiedName~SupplyArrVendorRestrictionTests|FullyQualifiedName~SupplyArrSupplyReadinessDashboardTests|FullyQualifiedName~SupplyArrSupplyReadinessCheckTests|FullyQualifiedName~SupplyArrVendorCatalogApiTests|FullyQualifiedName~SupplyArrProcurementExceptionTests|FullyQualifiedName~SupplyArrProcurementCoordinationWorkerTests|FullyQualifiedName~SupplyArrDemandProcessingWorkerTests|FullyQualifiedName~SupplyArrIntegrationEventTests|FullyQualifiedName~SupplyArrComplianceReportTests|FullyQualifiedName~SupplyArrVendorReportTests" --logger "console;verbosity=minimal"` - passed 55 tests.
- `npm test -- --run src/layouts/ProductWorkspaceLayout.test.tsx src/workspace/sections/CatalogSection.test.tsx src/workspace/sections/PurchasingSection.test.tsx src/workspace/sections/SettingsSection.test.tsx src/workspace/sections/ReportsSection.test.tsx src/pages/vendor-portal/VendorPortalPage.test.tsx src/components/VendorRestrictionsPanel.test.tsx src/components/SupplyReadinessDashboardPanel.test.tsx src/components/SupplyReadinessCheckPanel.test.tsx src/components/VendorCatalogApiPanel.test.tsx src/components/ProcurementCoordinationPanel.test.tsx src/components/DemandProcessingPanel.test.tsx src/components/PartSubstitutionsPanel.test.tsx` from `apps/supplyarr-frontend` - passed 12 files / 24 tests.
- `npm run test:theme` from `apps/supplyarr-frontend` - passed with no theme audit violations.

Deferred blockers:

- Advanced supplier collaboration and network depth (`SU-UND-001`, `SU-UND-005`, `SU-UND-006`, `SU-UND-010`, `SU-DEM-003`, `SU-DEM-007`, `SU-DEM-008`) remains retained scope beyond current vendor portal, onboarding, email, and catalog API flows. No consent-based supplier network, compliance passport, supplier development plan, or multi-tier supply-chain map was introduced.
- Advanced optimization and AI/proposal depth (`SU-DEM-001`, `SU-DEM-004`, `SU-DEM-005`, `SU-DEM-006`, `SU-DEM-009`, `SU-FND-020`) remains deferred. Existing reorder evaluation, snapshots, imports, reports, and procurement coordination are deterministic/operator-reviewed and must not be represented as autonomous procurement, contract intelligence, should-cost intelligence, dynamic discounting, or AI commitments.
- `SU-DEM-010` procurement control tower remains partial/retained. Current dashboards and exception queues provide owned context, but not a full cross-demand, supplier, contract, inventory, shipment, quality, and finance risk control tower with response-task orchestration.
- `SU-CUR-016` legacy WMS boundary debt remains active. SupplyArr still contains physical inventory/location/ledger/reservation surfaces until LoadArr migration or projection replacement is complete.
- RecordArr durable file retention, AssurArr quality/SCAR decisions, LoadArr physical execution, and ReportArr suite projections remain owner-specific dependencies outside this SupplyArr R12 pass.

R12 stage result: SupplyArr is clear for the R12 product gate with the deferred blockers above. Continue R12 with LoadArr next; do not advance to later-stage work until every applicable product completes R12.
