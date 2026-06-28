# MaintainArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `maintainarr` |
| Category | CMMS / EAM |
| Entry release | R3 — MaintainArr flagship operational slice |
| Completion release | R3 — MaintainArr flagship operational slice |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Assets, defects, inspections, preventive maintenance, work orders, readiness, downtime, and maintenance execution. |
| Roadmap slice | First flagship operational slice |
| Must not violate | Prove asset-to-work-to-evidence without stealing inventory, training, quality, or document truth. |
| Feature rows retained | 73 |
| Workflow rows retained | 14 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R3 | MaintainArr flagship operational slice | 38 | 14 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R3 unless they are only supporting another release gate.
- Common category baseline remains retained for R3.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## R0 Trust Gate pass

Status: Clear for R0 after focused launch/session and permission-gate hardening.

Completed blockers:

- Removed the stale MaintainArr session/me `hasMaintainArrAccess` success flag from API contracts, frontend types, and current tests.
- Stopped passing legacy NexArr launchable-product claims through MaintainArr handoff and session bootstrap responses. MaintainArr now returns a fixed ordinary-suite launch catalog and excludes Compliance Core from normal tenant product switching.
- Renamed the local authorization shim from entitlement wording to launch-context wording while preserving server-side action gates.
- Fixed the asset read/manage gates so active-tenant platform-admin sessions created through NexArr handoff can perform the same MaintainArr setup actions already expected by the product tests, without opening anonymous or UI-only success paths.

Files touched:

- `apps/maintainarr-api/MaintainArr.Api/Contracts/AuthContracts.cs`
- `apps/maintainarr-api/MaintainArr.Api/Services/HandoffAuthService.cs`
- `apps/maintainarr-api/MaintainArr.Api/Services/MaintainArrAuthorizationService.cs`
- `apps/maintainarr-api/MaintainArr.Api/Services/MaintainArrSuiteLaunchCatalog.cs`
- `apps/maintainarr-api/MaintainArr.Api/Services/MeService.cs`
- `apps/maintainarr-frontend/src/api/types.ts`
- `apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx`
- `apps/maintainarr-frontend/src/pages/assets/AssetCreatePage.test.tsx`
- `apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.test.tsx`
- `apps/maintainarr-frontend/src/pages/defects/DefectCreatePage.test.tsx`
- `apps/maintainarr-frontend/src/pages/parts-kits/PartsKitCreatePage.test.tsx`
- `apps/maintainarr-frontend/src/pages/pm-programs/PmProgramCreatePage.test.tsx`
- `apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.test.tsx`
- `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrHandoffApiTests.cs`

Tests run:

- `npm test -- ProductWorkspaceLayout.test.tsx AssetProfilePage.test.tsx AssetCreatePage.test.tsx DefectCreatePage.test.tsx WorkOrderCreatePage.test.tsx PmProgramCreatePage.test.tsx PartsKitCreatePage.test.tsx` from `apps/maintainarr-frontend` - passed 7 files / 15 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.MaintainArr.Auth.Tests.MaintainArrHandoffApiTests.Asset_registry_crud_happy_path" --logger "console;verbosity=minimal"` - passed 1 test after the asset manage gate fix.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrHandoffApiTests" --logger "console;verbosity=minimal"` - passed 10 tests.

Remaining blockers: None identified in this R0 slice.

R0 stage result: MaintainArr is clear to advance when the suite reaches the next stage gate.

## R1 Foundation spine pass

Status: Not applicable. MaintainArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R3.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no MaintainArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no MaintainArr rows for `R1`.
- MaintainArr's product FEATURESET and WORKFLOWS remain retained full scope, but they do not authorize starting R3 flagship CMMS work during the R1 suite stage.

Files touched:

- `docs/roadmap/products/maintainarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no MaintainArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. MaintainArr must wait for the suite to reach R3 before its flagship operational slice begins.

R1 stage result: MaintainArr is clear for the R1 suite gate as not applicable.

## R3 MaintainArr flagship operational slice pass

Status: Clear for R3 with an external RecordArr evidence-vault dependency carried forward.

R3 roadmap scope audited:

- Feature rows `MA-CUR-001` through `MA-CUR-017` cover the current durable MaintainArr asset, fieldset, readiness, meter, PM, inspection, defect, work-order, parts-demand, return-to-service, downtime, recall, enrichment, parts/vendor, quality-hold, compliance-mirror, import, notification, audit, and event surfaces.
- Feature rows `MA-COM-001` through `MA-COM-014` cover the common CMMS/EAM baseline for asset lifecycle, request intake, planning/scheduling, PM/condition-based maintenance, inspections, technician execution, parts/materials, vendor work, downtime, warranty/recall, reservation/readiness, reporting, mobile/offline, and safety/permit controls.
- Feature rows `MA-FND-006` through `MA-FND-011` and `MA-FND-016` cover saved views, bulk operations, import review, export/portability, notifications, APIs/webhooks/outbox, and professional reports.
- Workflow rows `MA-WF-001` through `MA-WF-014` cover asset creation/activation, work intake, inspection-to-defect, defect-to-work-order-to-return-to-service, PM forecast/generation/execution/deferral, meter threshold response, emergency breakdown, parts demand, vendor work, recall campaign, downtime/restoration, quality hold mirroring, reservation/readiness, and maintenance audit packages.

Completed fixes:

- Updated the cross-product handoff test fixture in `OrdArrCustomArrHandoffTests` to construct `CustomArrStore` with its current durable `CustomArrDbContext`, unblocking MaintainArr backend test compilation without reverting CustomArr's persistence boundary.
- Changed MaintainArr tenant defaults so RecordArr packet handoff is off by default (`SendCompletedPacketsToRecordArr = false`, `EnableRecordArrDocumentPackets = false`). The settings remain configurable, but the product no longer defaults to implying completed packet delivery to a RecordArr vault that is still blocked by its durable-store migration.

Cross-product notes:

- MaintainArr owns maintenance execution, operational attachments, readiness state, work orders, downtime, audit events, and maintenance reports.
- SupplyArr remains the owner of inventory/procurement truth. MaintainArr parts demand and supply-readiness tests verify request/status coordination rather than inventory ownership transfer.
- StaffArr remains the owner of people and internal locations. MaintainArr technician/site references stay as references or snapshots, not StaffArr source truth.
- Compliance Core remains the owner of regulatory meaning and readiness guidance. MaintainArr consumes gates and mirrors regulatory keys without owning rule interpretation.
- RecordArr remains the owner of retained documents and production evidence vault truth, but its durable DMS blocker remains unresolved. MaintainArr local evidence storage and work-order/defect/inspection evidence records are operational maintenance attachments, not production-authoritative RecordArr retained evidence.

Files touched:

- `apps/maintainarr-api/MaintainArr.Api/Services/MaintainArrTenantSettingsService.cs`
- `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrTenantSettingsRulesTests.cs`
- `tests/STLCompliance.MaintainArr.Auth.Tests/OrdArrCustomArrHandoffTests.cs`
- `docs/roadmap/products/maintainarr.md`

Tests run:

- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - timed out before completion; split R3 backend clusters passed as listed below.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~AssetReadinessRulesTests|FullyQualifiedName~AssetStatusRollupRulesTests|FullyQualifiedName~DowntimeDeepLinkBuilderTests" --logger "console;verbosity=minimal"` - passed 26 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrHandoffApiTests" --logger "console;verbosity=minimal"` - passed 10 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrAssetReadinessTests" --logger "console;verbosity=minimal"` - passed 14 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrAssetStatusRollupWorkerTests" --logger "console;verbosity=minimal"` - passed 7 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrAssetDowntimeTests" --logger "console;verbosity=minimal"` - passed 10 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrAssetBulkImportTests|FullyQualifiedName~MaintainArrCatalogFieldsetControlledTests|FullyQualifiedName~MaintainArrReferenceIntegrationAuthTests|FullyQualifiedName~MaintainArrMeterTrackingTests" --logger "console;verbosity=minimal"` - passed 35 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrPmProgramTests|FullyQualifiedName~MaintainArrPmDueScanWorkerTests|FullyQualifiedName~MaintainArrInspectionTemplateTests|FullyQualifiedName~MaintainArrInspectionRunTests" --logger "console;verbosity=minimal"` - passed 36 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrDefectTests|FullyQualifiedName~MaintainArrDefectEvidenceTests|FullyQualifiedName~MaintainArrDefectEscalationWorkerTests|FullyQualifiedName~DefectEscalationRulesTests" --logger "console;verbosity=minimal"` - passed 21 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrWorkOrderTests|FullyQualifiedName~WorkOrderStatusRulesTests|FullyQualifiedName~MaintainArrWorkOrderLaborEvidenceTests" --logger "console;verbosity=minimal"` - passed 46 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrSupplyArrPartsDemandTests|FullyQualifiedName~MaintainArrWorkOrderSupplyReadinessTests|FullyQualifiedName~MaintainArrTechnicianRefTests|FullyQualifiedName~MaintainArrStaffarrTechnicianSyncTests" --logger "console;verbosity=minimal"` - passed 26 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrMaintenanceHistoryTests|FullyQualifiedName~MaintenanceHistoryRulesTests|FullyQualifiedName~MaintainArrMaintenanceHistoryRollupWorkerTests|FullyQualifiedName~MaintainArrMaintenanceReportTests" --logger "console;verbosity=minimal"` - passed 21 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrNotificationTests|FullyQualifiedName~MaintenanceNotificationRulesTests|FullyQualifiedName~MaintainArrPlatformEventTests|FullyQualifiedName~MaintenancePlatformEventRulesTests|FullyQualifiedName~MaintainArrFieldInboxTests" --logger "console;verbosity=minimal"` - passed 36 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrAuditPackageTests|FullyQualifiedName~MaintainArrAuditPackageGenerationTests|FullyQualifiedName~MaintainArrAuditPackageGenerationRulesTests|FullyQualifiedName~MaintainArrComplianceReportTests|FullyQualifiedName~MaintainArrExecutiveReportTests|FullyQualifiedName~MaintainArrEntityBulkExportTests" --logger "console;verbosity=minimal"` - passed 34 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrTenantSettingsRulesTests|FullyQualifiedName~SuiteSchedulingContractsTests|FullyQualifiedName~MaintainArrSmartImportCommitHandlerTests|FullyQualifiedName~OrdArrCustomArrHandoffTests|FullyQualifiedName~VoiceNumericNormalizerTests" --logger "console;verbosity=minimal"` - passed 20 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrTenantSettingsRulesTests" --logger "console;verbosity=minimal"` - passed 6 tests after the RecordArr default repair.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~AssetReservation|FullyQualifiedName~VendorWork|FullyQualifiedName~MaintenancePartsKit" --logger "console;verbosity=minimal"` - no direct backend tests matched those filters.
- `npm test` from `apps/maintainarr-frontend` - passed 52 files / 145 tests.
- `npm test -- SettingsSection.test.tsx` from `apps/maintainarr-frontend` - passed 1 file / 4 tests after the RecordArr default repair.

Remaining blockers:

- No unresolved MaintainArr-owned R3 blockers were identified in this pass.
- External carried blocker: RecordArr's durable retained-evidence persistence remains unresolved. MaintainArr is clear for operational R3 maintenance execution, but any workflow that requires production-authoritative retained evidence must continue to treat RecordArr packet handoff as pending until RecordArr closes its DMS blocker.
- Test gap: reservation, vendor-work, and parts-kit surfaces have frontend and service coverage through the existing suite, but no direct backend tests matched the explicit `AssetReservation`, `VendorWork`, or `MaintenancePartsKit` filters in this test project.

R3 stage result: MaintainArr is clear for R3, with the external RecordArr durable evidence-vault blocker carried forward and RecordArr packet handoff disabled by default.

## R12 Expansion pass

Status: Clear for R12 with advanced CMMS/EAM expansion scope explicitly deferred.

R12 scope audited:

- MaintainArr has 35 R12 feature rows and no R12 workflow rows in the roadmap rollout maps.
- The audited R12 rows cover one-tap field execution, voice-guided capture, explainable readiness, quick-create reference completion, small-fleet/mixed-asset mode, maintenance-to-procurement custody visibility, affordable condition monitoring, evidence quality coaching, guided troubleshooting, schedule/backlog transparency, contractor access, operations feedback, predictive maintenance, strategy optimization, digital twin context, augmented reality, reliability engineering, multi-site planning, warranty recovery, recall/service-bulletin intelligence, cost/lifecycle forecasting, remote expert collaboration, and shared foundation behaviors.
- Current represented R12-adjacent slices are bounded: voice guidance for inspection prompts, quick-create for asset/part/site references, asset reservation lifecycle, vendor-work portal access, maintenance history/reliability reporting, and RecordArr packet settings. They remain operational MaintainArr slices and do not claim full predictive maintenance, AR, digital twin, warranty recovery, remote expert, or cost-forecasting completion.

Cross-product notes:

- RecordArr remains an external carried dependency for production-authoritative retained evidence. MaintainArr continues to keep RecordArr document packets disabled by default until RecordArr closes its durable DMS blocker.
- LoadArr/SupplyArr continue to own inventory, procurement, and custody truth. MaintainArr parts demand and kit/reference slices provide maintenance context without becoming stock ledger or sourcing truth.
- StaffArr/TrainArr continue to own person/location and qualification truth. MaintainArr voice guidance, reservations, and vendor work consume or reference those domains without copying ownership.

Files touched:

- `docs/roadmap/products/maintainarr.md`

Tests run:

- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrTechnicianRefTests|FullyQualifiedName~MaintainArrTenantSettingsRulesTests" --logger "console;verbosity=minimal"` - passed, 10 tests.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~MaintainArrAssetDowntimeTests" --logger "console;verbosity=minimal"` - passed, 10 tests.
- `npm test -- --run src/inspections/voiceGuidance.test.ts src/components/InspectionRunnerPanel.test.tsx src/pages/defects/DefectCreatePage.test.tsx src/pages/parts-kits/PartsKitCreatePage.test.tsx src/pages/pm-programs/PmProgramCreatePage.test.tsx src/pages/vendor-portal/VendorPortalPage.test.tsx src/components/WorkOrderVendorWorkPanel.test.tsx src/pages/settings/SettingsSection.test.tsx` from `apps/maintainarr-frontend` - passed, 7 files / 14 tests.
- `npm run test:theme` from `apps/maintainarr-frontend` - passed with no theme audit violations.

Additional verification:

- A broader backend filter covering technician, reservations, settings, parts demand, and work orders timed out locally and is not used as passing evidence.
- Split backend filters for `MaintainArrSupplyArrPartsDemandTests` and `MaintainArrWorkOrderTests` also timed out locally and are not used as passing evidence in this R12 pass.

Remaining blockers / explicit deferrals:

- Deferred to later R12-ready slices: full one-tap technician execution, full voice-guided work capture, complete mixed-fleet mode, field evidence quality coaching, guided troubleshooting/knowledge reuse, predictive maintenance, strategy optimization, digital twin, augmented reality, reliability engineering toolkit depth, multi-site planning, warranty recovery automation, service-bulletin intelligence, maintenance cost/lifecycle forecasting, remote expert collaboration, and broader Field Companion execution.
- External carried blocker: RecordArr durable retained-evidence persistence remains unresolved and must continue to be called out for workflows requiring production-authoritative document packets or evidence archives.

R12 product result: MaintainArr is clear for the R12 suite gate. Continue R12 with TrainArr; do not advance beyond R12 until every applicable product clears this stage.

## Source docs

- [Feature catalog](../../products/maintainarr/FEATURESET.md)
- [Workflow catalog](../../products/maintainarr/WORKFLOWS.md)
- [Product manifest](../../products/maintainarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
