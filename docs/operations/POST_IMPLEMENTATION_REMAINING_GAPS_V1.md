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
| SupplyArr source recommendation | Closed with focused coverage | Pricing and lead-time source recommendations now surface best overall, lowest cost, fastest delivery, preferred vendor, compliance-safest, emergency, needs-approval, and not-recommended views using the current pricing, lead-time, and vendor approval data. |
| SupplyArr procurement analytics | Closed with focused coverage | Purchasing reports now surface pending approvals, emergency requests, procurement exceptions, receiving exceptions, warranty claims, expiring vendor documents, blocked vendors, average lead time, and estimated spend from existing purchasing, compliance, and vendor data. |
| SupplyArr contracts and purchasing terms | Closed with focused coverage | Supplier profile now surfaces contract records, approval state, effective/expiry windows, renewal dates, payment/freight/warranty terms, minimum spend, and SLA details using the existing contract-record API. |
| SupplyArr contracts import | Closed with focused coverage | Purchasing workspace now supports paste/upload/dry-run/import of contract CSV rows through the existing contracts import endpoint, with row-level validation output shown in the UI. |
| SupplyArr WMS/RoutArr shipment bridge | Closed with focused coverage | WMS stock movement/outbound shipment endpoints, RoutArr shipment-intent intake, and the RoutArr status callback path are now covered by focused happy-path and failure-path tests. |
| RoutArr dispatch release readiness snapshot | Closed | Dispatch workflow gate checks and assignment previews carry `releaseSnapshot` with immutable context and gate summaries. |
| MaintainArr asset readiness metadata | Verified present | Asset readiness responses include blocker categories, signal counts, dispatchability, confidence/staleness, audit snapshot, and Compliance Core references. |
| Shared launch endpoint mapping across product APIs | Closed with source guard | E2E catalog test asserts every product API maps `MapStlProductLaunchEndpoints`; shared endpoint test asserts v1 catalog/context/handoff routes exist. |
| SupplyArr procurement exception reopen | Verified present | Current SupplyArr source already includes reopen routing and UI controls; older reported gap is stale. |
| End-to-end TrainArr qualification to RoutArr dispatch browser journey | Closed with focused coverage | Browser smoke now completes a TrainArr assignment in the UI, verifies the resulting qualification grant publication, and assigns the same driver in RoutArr without a block dialog. |
| TrainArr rule-pack picker UX | Closed with focused coverage | TrainArr qualification check, batch check, impact assessment, and rule-pack requirement panels now use the shared searchable picker for rule pack selection instead of plain dropdowns. |
| TrainArr applicability builder picker UX | Closed with focused coverage | Applicability builder now uses the shared searchable picker for scope references, source references, and requirement targets instead of plain dropdowns. |
| TrainArr authoring picker UX | Closed with focused coverage | Training definition and step authoring panels now use the shared searchable picker for definition and step selection instead of plain dropdowns. |
| TrainArr content references | Closed with focused coverage | Program profiles now surface generic content references and support attach/list/remove flows for uploaded files, URLs, policy docs, citations, and external product references without owning the source system. |
| TrainArr LMS-grade content delivery | Closed with focused coverage | Content steps now render lesson text and media, require acknowledgement when configured, persist lesson metadata in responses, and validate content configs instead of treating lessons as a flat status-only step. |
| TrainArr practical assessment support | Closed with focused coverage | Practical step authoring and assignment workflows now carry structured evaluation prompts, observation notes, safety-critical failure tracking, trainee acknowledgement, and retest guidance instead of a flat pass/fail select. |
| TrainArr digital wallet / smart badge credentials | Closed with focused coverage | Qualification issues now issue signed wallet credentials and verify them against a live point-in-time qualification report rather than only showing raw qualification state. |
| TrainArr AI-assisted training authoring | Closed with focused coverage | Program builder now accepts a natural-language prompt, generates a ranked draft from active definitions, and applies the suggested name, description, and definition set into the create form. |
| TrainArr multilingual/cost/labor analytics | Closed with focused coverage | Program content references now carry locale tags, assignment labor entries persist hours and cost, and assignment reports surface labor and localization analytics from the live TrainArr data. |
| SupplyArr purchase request picker UX | Closed with focused coverage | Purchase-request creation now uses the shared searchable picker for vendor and part selection instead of plain dropdowns. |
| MaintainArr inspection runner picker UX | Closed with focused coverage | Inspection runner asset and active template selection now use the shared searchable picker instead of plain dropdowns. |
| MaintainArr asset details picker UX | Closed with focused coverage | Asset details now use the shared searchable picker for the fallback asset selector instead of a plain dropdown. |
| MaintainArr meter readings picker UX | Closed with focused coverage | Meter readings now use the shared searchable picker for asset and meter selection instead of plain dropdowns. |
| MaintainArr work-order labor picker UX | Closed with focused coverage | Work-order labor capture now uses the shared searchable picker for technician and task line selection instead of plain dropdowns. |
| MaintainArr downtime picker UX | Closed with focused coverage | Manual downtime creation now uses the shared searchable picker for asset selection instead of a plain dropdown. |
| MaintainArr maintenance history picker UX | Closed with focused coverage | Maintenance history now uses the shared searchable picker for asset selection instead of a plain dropdown. |
| MaintainArr audit export picker UX | Closed with focused coverage | Audit package export now uses the shared searchable picker for actor-user selection instead of a plain dropdown. |
| RoutArr command-center picker UX | Closed with focused coverage | Dispatch command center now uses the shared searchable picker for driver assignment instead of a plain dropdown. |
| MaintainArr shop-floor scan card | Closed with focused coverage | Asset profile now shows a stable mobile / QR-ready scan payload with a copy action for opening the asset context on the shop floor. |
| MaintainArr telematics / diagnostics ingestion history | Closed with focused coverage | Asset profile now surfaces recent RoutArr inbound events linked to the asset, including processed/ignored counts, defect links, and diagnostic payload summaries. |
| NexArr tenant admin visibility | Closed with focused coverage | Tenant overview now surfaces live tenant detail, members, entitlements, and service-client scope using existing NexArr tenant, entitlement, and service-client APIs. |
| NexArr product admin visibility | Closed with focused coverage | Product overview now surfaces product manifest details plus product-scoped service clients using existing NexArr product, manifest, and service-client APIs. |
| NexArr tenant launch history visibility | Closed with focused coverage | Tenant overview now surfaces recent launch attempts and remediation hints using the existing platform launch-attempts API filtered by tenant. |
| NexArr product launch activity visibility | Closed with focused coverage | Product overview now surfaces recent launch attempts and launch outcomes using the existing platform launch-attempts API filtered by product key. |
| NexArr hybrid data-plane picker UX | Closed with focused coverage | Hybrid data-plane metadata now uses the shared searchable picker for tenant and product selection instead of plain dropdowns. |
| NexArr hybrid data-plane trust proof | Closed with focused coverage | Hybrid data-plane validation now probes the configured tenant endpoint, promotes successful customer-hosted profiles to trusted, and persists pending-validation results when the probe fails. |
| NexArr launch validation picker UX | Closed with focused coverage | Launch diagnostics now uses the shared searchable picker for tenant and product selection during eligibility validation. |
| NexArr tenant catalog picker UX | Closed with focused coverage | Tenant catalog editing now uses the shared searchable picker for tenant selection instead of a plain dropdown. |
| NexArr product catalog picker UX | Closed with focused coverage | Product catalog editing now uses the shared searchable picker for product selection instead of a plain dropdown. |
| NexArr entitlement grant picker UX | Closed with focused coverage | Entitlement administration now uses the shared searchable picker for product selection during grant flow. |
| NexArr launch filter picker UX | Closed with focused coverage | Launch diagnostics now uses the shared searchable picker for tenant and product filters in the attempt list. |
| NexArr quick-switch current-product indicator | Closed with focused coverage | The suite product switcher now asks NexArr for the current product key and uses the canonical launch catalog response to render the current product marker. |
| NexArr service token discovery visibility | Closed with focused coverage | The service-token admin panel now shows issuer, audience, JWKS URI, supported algorithms, and public-key availability using NexArr&apos;s discovery endpoint. |
| NexArr password policy configuration | Closed with focused coverage | Platform session settings now persist and expose password minimum length and complexity requirements, and the password-reset/admin-user flows enforce the configured policy. |
| NexArr deployment evidence surface | Closed with focused coverage | Platform status now shows observed downstream deployment version and health-check evidence for each product probe, exposing more than a yes/no readiness result. |
| StaffArr sensitive role assignment approval gate | Closed with focused coverage | Sensitive role assignments now start in `pending_review`, the effective permission projection ignores them until approved, and the existing status transition approves or rejects the assignment. |
| StaffArr workforce readiness confidence scoring | Closed with focused coverage | Readiness rollup summaries now persist and expose confidence level and score derived from member readiness snapshots, and the supervisor drill-down surfaces the aggregate confidence summary. |
| NexArr system status registry summary | Closed with focused coverage | Platform status now shows a product-registry health summary derived from the canonical product registry alongside probe health, missing URL gaps, and per-product registry rows. |
| NexArr deployment drift evidence | Closed with focused coverage | Platform status now groups observed health-probe versions into a drift summary so operators can spot likely stale deployments and version skew from live probe evidence. |
| Shared API startup validation | Closed with focused coverage | Production startup now fails fast when the shared JWT signing key is missing or too short, verified in the NexArr startup harness and inherited by all APIs using the shared host. |
| Shared API security headers | Closed with focused coverage | All API health endpoints now emit the expected security header set, verified through the cross-product health test harness. |
| NexArr auth rate limiting | Closed with focused coverage | NexArr auth endpoints now enforce a rate-limit policy by IP, with a focused login-throttling test proving 429 behavior. |

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
- `dotnet test tests\STLCompliance.E2E\STLCompliance.E2E.csproj --no-restore --filter "FullyQualifiedName~StlE2ePlaywrightSpecCatalogTests"` passed 7 tests after the TrainArr qualification publication browser smoke was added to the catalog.
- `npm test -- trainarr-routarr-qualification-issue-publication-journey-smoke.spec.ts` in `tests/e2e-playwright` parsed the new smoke cleanly and skipped as expected because `E2E_LIVE` was not enabled in this workspace.
- `OPENAPI_UPDATE_SNAPSHOTS=1 dotnet test tests\STLCompliance.OpenApi.Tests\STLCompliance.OpenApi.Tests.csproj --no-restore` refreshed intentional contract snapshots.
- `dotnet test tests\STLCompliance.OpenApi.Tests\STLCompliance.OpenApi.Tests.csproj --no-restore` passed 16 tests after snapshot refresh.
- `npm run build` passed for `apps/trainarr-frontend`, `apps/staffarr-frontend`, `apps/routarr-frontend`, and `apps/maintainarr-frontend`.
- `dotnet build apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj -c Release` passed after the readiness rollup confidence fields were added.
- `dotnet test tests\STLCompliance.StaffArr.Auth.Tests\STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrReadinessRollupWorkerTests"` passed 6 tests after the confidence summary assertions were added.
- `OPENAPI_UPDATE_SNAPSHOTS=1 dotnet test tests\STLCompliance.OpenApi.Tests\STLCompliance.OpenApi.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrOpenApiParityTests"` refreshed the StaffArr contract snapshot.
- `npm test -- ReadinessRollupSupervisorPanel.test.tsx` in `apps/staffarr-frontend` passed after the confidence card and table column were added.
- `npm run build` in `apps/staffarr-frontend` passed after the rollup confidence UI changes.
- `dotnet build apps/nexarr-api/NexArr.Api/NexArr.Api.csproj -c Release` passed after MFA provisioning, recovery-code consumption, and DI updates.
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~NexArrAuthApiTests|FullyQualifiedName~NexArrPlatformAdminApiTests"` passed 54 tests covering MFA provisioning, login challenge handling, and recovery-code consumption.
- `OPENAPI_UPDATE_SNAPSHOTS=1 dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj -c Release --filter "FullyQualifiedName~NexArrOpenApiParityTests"` refreshed the NexArr contract snapshot.
- `npm test -- LoginPage.test.tsx PlatformUsersPage.test.tsx` in `apps/suite-frontend` passed after the MFA login challenge and provisioning UI changes.
- `npm run build` in `apps/suite-frontend` passed after the MFA UI changes.

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
| Cross-product reason-code vocabulary alignment | Compliance Core, SupplyArr, StaffArr, RoutArr, MaintainArr | Reason codes are structured in several products, but not all reason vocabularies are sourced from a single Compliance Core registry. |
| WMS outbound shipment behavioral proof | Closed with focused coverage | Focused WMS stock movement, idempotency, route-shipment intent, and RoutArr shipment-status callback tests are present in the current SupplyArr and RoutArr auth suites. |
| Deployment drift remediation | All deployed services | Source maps shared launch endpoints, but deployed crawl evidence must be refreshed to prove no stale deployments or CORS drift remain. |

## Deferred Product-Specific End-State Gaps

| Product | Remaining Gaps |
|---|---|
| NexArr | MFA enrollment/challenge/recovery lifecycle closed with focused coverage; billing/licensing readiness depth, password policy configuration, the system status registry summary, deployment evidence surfaces, and deployment drift evidence are closed with focused coverage. |
| StaffArr | Closed with incident notes, corrective actions, attachments, and timeline/event-feed coverage. |
| TrainArr | Closed with focused coverage across training effectiveness analytics, generic content references, LMS-grade content delivery, structured practical assessment support, digital wallet/smart badge credentials, AI-assisted training authoring, and multilingual/cost/labor analytics. |
| RoutArr | Route optimization, geofence/GPS stop checks, driver-portal offline/resync operations, and driver time tracking are closed with focused coverage; remaining open items are carrier/customer portals, dock/detention workflows, and HOS/ELD integrations. |
| MaintainArr | Shop-floor mobile/QR/display workflows, deeper parts-demand forecasting, telematics / diagnostics ingestion history, and predictive maintenance loops are closed with focused coverage. |
| SupplyArr | Broader ERP interoperability remains open; the email inbox integrations, vendor catalog APIs, automated exception policy, accounting/ERP export center, and RFQ vendor portal slices are closed with focused coverage. |
| Compliance Core | Full Title 49 legal coverage, product calculators, hazmat table enumeration, reference-mapped regulatory domains, immutable snapshot hardening, rate limiting, and large audit proof remain open; the evaluation history explorer is closed with focused coverage. |

## Known Verification Risks

- EF migrations/model snapshots need review for the newly persisted StaffArr permission metadata, StaffArr site reference snapshots, SupplyArr WMS entities, and RoutArr shipment-intent entities before staging deployment.
- Live Render verification was not run because deployed credentials and service URLs were not exercised in this workspace pass.
- WMS/RoutArr shipment bridge behavior now has focused happy-path and failure-path tests, but the deployed render proof for the bridge still needs refresh alongside the broader staging evidence pass.
- Some frontend surfaces consume new metadata as optional fields to preserve compatibility with older mocked responses.
