# STL Compliance / Arr Suite
# Post-Implementation Remaining Gaps Register (V1)

Generated: 2026-06-02

## Purpose

This register captures the cross-product closure pass that followed the feature-set markdown review. It separates:

- cross-product gaps closed or verified in this pass;
- post-implementation verification evidence;
- deferred cross-product gaps;
- deferred product-specific end-state gaps;
- known deployment, migration, and verification risks.

This document is the post-implementation gap source of truth. The product feature-set markdown files remain the long-range target references.

## Closed Or Verified In This Pass

| Area | Status | Evidence |
|---|---:|---|
| TrainArr qualification checks carry catalog snapshots | Closed | Single and batch check responses now include `qualificationCatalog` with source product/entity/id, label snapshot, status snapshot, verification time, and sync timestamp. |
| TrainArr local qualification state carries source timestamps | Closed | `localQualification` now includes qualification name, issued/expiry dates, and last verification timestamp. |
| StaffArr product permission catalog sync | Closed | Product service-token sync imports product-owned permission keys into permission templates with product key, scope, sensitivity, status, and `lastSyncedAt`. |
| StaffArr role templates expose permission catalog metadata | Closed | Permission and role-template responses include product ownership, sensitivity, and sync timestamps without changing assignment ids. |
| StaffArr product incident source snapshots | Closed | Product incident intake/list/detail responses expose `sourceSnapshot` using persisted source product/id/event/reference fields. |
| StaffArr active site lookup contract | Closed | `/api/v1/integrations/sites` list/detail endpoints expose active site org units through the shared StaffArr site lookup client and `staffarr.sites.read` scope. |
| Product site-reference snapshots | Closed with focused coverage | Compliance Core HazCom, MaintainArr assets, SupplyArr inventory locations, TrainArr applicability profiles, and RoutArr route stops now resolve StaffArr site references and carry stable site snapshots. |
| SupplyArr PO/receiving baseline failures | Verified closed | Fresh-build PO/receiving happy-path tests passed after inventory locations were updated to use StaffArr site references. |
| SupplyArr demand processing and supply readiness callbacks | Verified closed | Focused demand-processing worker and supply-readiness tests passed on current source. |
| SupplyArr WMS/RoutArr shipment bridge | Implemented, behavior coverage pending | WMS stock movement/outbound shipment endpoints and RoutArr shipment-intent intake compile and are reflected in OpenAPI snapshots; dedicated WMS behavior tests remain deferred. |
| RoutArr dispatch release readiness snapshot | Closed | Dispatch workflow gate checks and assignment previews carry `releaseSnapshot` with immutable context and gate summaries. |
| MaintainArr asset readiness metadata | Verified present | Asset readiness responses include blocker categories, signal counts, dispatchability, confidence/staleness, audit snapshot, and Compliance Core references. |
| Shared launch endpoint mapping across product APIs | Closed with source guard | E2E catalog test asserts every product API maps `MapStlProductLaunchEndpoints`; shared endpoint test asserts v1 catalog/context/handoff routes exist. |
| SupplyArr procurement exception reopen | Verified present | Current SupplyArr source already includes reopen routing and UI controls; older reported gap is stale. |

## Verification Evidence

Commands run on 2026-06-02:

- `dotnet restore STLCompliance.slnx --verbosity:minimal` succeeded after `LoadArr.Api` needed a missing assets file.
- `dotnet build STLCompliance.slnx --no-restore --verbosity:minimal` passed.
- `dotnet test tests\STLCompliance.StaffArr.Auth.Tests\STLCompliance.StaffArr.Auth.Tests.csproj --no-restore --filter "StaffArrIntegrationPermissionCheckTests|StaffArrTrainArrQualificationCheckTests|StaffArrTrainArrQualificationBatchCheckTests"` passed 26 tests.
- `dotnet test tests\STLCompliance.SupplyArr.Auth.Tests\STLCompliance.SupplyArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~SupplyArrHandoffApiTests.Purchase_order_from_approved_pr_approve_issue_happy_path|FullyQualifiedName~SupplyArrHandoffApiTests.Receiving_against_issued_po_posts_stock_happy_path|FullyQualifiedName~SupplyArrHandoffApiTests.Receiving_v1_against_issued_po_posts_stock_happy_path"` passed 3 tests.
- `dotnet test tests\STLCompliance.SupplyArr.Auth.Tests\STLCompliance.SupplyArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~SupplyArrDemandProcessingWorkerTests|FullyQualifiedName~SupplyArrSupplyReadinessCheckTests"` passed 21 tests.
- `dotnet test tests\STLCompliance.RoutArr.Auth.Tests\STLCompliance.RoutArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~RoutArrDispatchWorkflowGateTests|FullyQualifiedName~DispatchWorkflowGateRulesTests|FullyQualifiedName~DispatchAssignmentValidationRulesTests|FullyQualifiedName~RoutArrSupplyArrPartsDemandTests"` passed 17 tests.
- `dotnet test tests\STLCompliance.MaintainArr.Auth.Tests\STLCompliance.MaintainArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~MaintainArrSupplyArrPartsDemandTests"` passed 12 tests.
- `dotnet test tests\STLCompliance.MaintainArr.Auth.Tests\STLCompliance.MaintainArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~MaintainArrAssetReadinessTests|FullyQualifiedName~AssetReadinessRulesTests"` passed 33 tests.
- `dotnet test tests\STLCompliance.ComplianceCore.Auth.Tests\STLCompliance.ComplianceCore.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~ComplianceCoreSdsHazComRuleVersionTests"` passed 5 tests.
- `dotnet test tests\STLCompliance.E2E\STLCompliance.E2E.csproj --no-restore --filter "StlProductLaunchEndpointMappingCatalogTests"` passed 8 tests.
- `OPENAPI_UPDATE_SNAPSHOTS=1 dotnet test tests\STLCompliance.OpenApi.Tests\STLCompliance.OpenApi.Tests.csproj --no-restore` refreshed intentional contract snapshots.
- `dotnet test tests\STLCompliance.OpenApi.Tests\STLCompliance.OpenApi.Tests.csproj --no-restore` passed 16 tests after snapshot refresh.
- `npm run build` passed for `apps/trainarr-frontend`, `apps/staffarr-frontend`, `apps/routarr-frontend`, and `apps/maintainarr-frontend`.

Known verification warnings observed:

- Repeated `Microsoft.EntityFrameworkCore.Relational` 10.0.4 versus 10.0.8 assembly conflict warnings in test projects.
- Existing `NU1510` health-check package prune warnings in `STLCompliance.Shared`.
- Existing xUnit analyzer warnings in unrelated tests.
- Frontend build chunk-size warnings for TrainArr, RoutArr, and MaintainArr.

## Deferred Cross-Product Gaps

| Gap | Product Touchpoints | Remaining Work |
|---|---|---|
| Unified deployed staging proof | All product APIs/frontends | Run Render ship-gate/live checks with credentials, capture CORS, launch catalog, auth, and service-token evidence for every deployed product. |
| Multi-tenant soak and large-audit proof | All products | Run multi-tenant data isolation soak, large audit exports, rate limiting, and high-volume readiness/dispatch workflows. |
| Full cross-product selectable record pickers | All products | Site and qualification references advanced in this pass, but richer owning-product search APIs are still needed for every entity type. |
| End-to-end TrainArr qualification to RoutArr dispatch browser journey | TrainArr, StaffArr, RoutArr | Backend gate pieces are covered, but a full browser journey from qualification issue/publication through dispatch eligibility remains deferred. |
| Cross-product reason-code vocabulary alignment | Compliance Core, SupplyArr, StaffArr, RoutArr, MaintainArr | Reason codes are structured in several products, but not all reason vocabularies are sourced from a single Compliance Core registry. |
| WMS outbound shipment behavioral proof | SupplyArr, RoutArr | Add focused WMS stock movement, idempotency, route-shipment intent, and RoutArr shipment-status callback tests. |
| Deployment drift remediation | All deployed services | Source maps shared launch endpoints, but deployed crawl evidence must be refreshed to prove no stale deployments or CORS drift remain. |

## Deferred Product-Specific End-State Gaps

| Product | Remaining Gaps |
|---|---|
| NexArr | MFA enrollment/challenge/recovery lifecycle, billing/licensing readiness depth, hybrid data-plane trust proof, platform-level deployment diagnostics maturity. |
| StaffArr | Fine-grained approval/expiration workflows for sensitive product permissions, richer product incident lifecycle callbacks, workforce readiness confidence scoring beyond current rollups. |
| TrainArr | LMS-grade content delivery, AI-assisted training authoring, digital wallet/smart badge credentials, simulation/practical assessment support, multilingual/cost/labor analytics. |
| MaintainArr | Full telematics and diagnostics ingestion, shop-floor mobile/QR/display workflows, predictive maintenance loops, deeper parts-demand forecasting. |
| RoutArr | Route optimization, telematics/GPS/geofence intelligence, carrier/customer portals, dock/detention workflows, HOS/ELD integrations, barcode/offline operations. |
| SupplyArr | Vendor scorecards, ERP integrations, vendor portal, predictive stockout/source recommendation, broader procurement analytics and automated exception policy. |
| Compliance Core | Full Title 49 legal coverage, product calculators, hazmat table enumeration, reference-mapped regulatory domains, immutable snapshot hardening, rate limiting, and large audit proof. |

## Known Verification Risks

- EF migrations/model snapshots need review for the newly persisted StaffArr permission metadata, StaffArr site reference snapshots, SupplyArr WMS entities, and RoutArr shipment-intent entities before staging deployment.
- Live Render verification was not run because deployed credentials and service URLs were not exercised in this workspace pass.
- WMS/RoutArr shipment bridge behavior is compiled and contract-snapshotted, but not yet backed by focused happy-path and failure-path tests.
- Some frontend surfaces consume new metadata as optional fields to preserve compatibility with older mocked responses.
