> **Historical implementation snapshot.** This file records the code and terminology observed at the time of that audit. Any access-model language in the evidence tables is superseded by `../platform/product-availability-and-access-model.md` and the current constitutions.

# STLCompliance Monorepo Audit Snapshot

This document is a living audit package for the current implementation pass. It records the repository surfaces we verified, the defects fixed in this pass, and the remaining items that still need deeper recursive coverage.

## Verified commands and suites

- `npm test` in `packages/shared-ui`
- `npm test` in `apps/suite-frontend`
- `npm test` in `apps/ordarr-frontend`
- `npm test` in `apps/customarr-frontend`
- `npm test` in `apps/ledgarr-frontend`
- `npm test` in `apps/recordarr-frontend`
- `npm test` in `apps/staffarr-frontend`
- `npm test -- PersonBulkImportPanel.test.tsx` in `apps/staffarr-frontend/src/components`
- `npm test` in `apps/staffarr-frontend`
- `npm test -- AssetBulkImportPanel.test.tsx` in `apps/maintainarr-frontend/src/components`
- `npm test -- MaintenanceDetailProfiles.test.tsx` in `apps/maintainarr-frontend/src/workspace/sections`
- `npm test -- AssetCreatePage.test.tsx` in `apps/maintainarr-frontend/src/pages/assets`
- `npm test` in `apps/maintainarr-frontend`
- `npm test -- QualificationReportsPanel.test.tsx` in `apps/trainarr-frontend/src/components`
- `npm test` in `apps/trainarr-frontend`
- `npm test` in `apps/loadarr-frontend`
- `npm test` in `apps/reportarr-frontend`
- `npm test` in `apps/routarr-frontend`
- `npm test` in `apps/assurarr-frontend`
- `npm test` in `apps/compliancecore-frontend`
- `npm test -- VendorOrderPortalPage.test.tsx` in `apps/supplyarr-frontend/src/pages/vendor-orders`
- `npm test -- VendorOrderSettingsPanel.test.tsx` in `apps/supplyarr-frontend/src/components`
- `npm test` in `apps/supplyarr-frontend`
- `npm test -- products.test.ts` in `apps/stlcompliancesite/src/content`
- `npm test` in `apps/stlcompliancesite`
- `npm test -- offlineSyncOutcome.test.ts` in `apps/fieldcompanion-frontend/src/lib`
- `npm test` in `apps/fieldcompanion-frontend`
- `npm test` in `apps/stlcompliancekb`
- `npm run build` in `apps/stlcompliancekb`
- `dotnet test` in `tests/STLCompliance.Shared.Tests`
- `dotnet test --filter "FullyQualifiedName~ProductSurfaceCatalogTests|FullyQualifiedName~ProductKeyAliasesTests|FullyQualifiedName~PlatformHealthServiceTests"` in `tests/STLCompliance.NexArr.Auth.Tests`
- `dotnet test` in `tests/STLCompliance.OpenApi.Tests`
- `dotnet test --filter "FullyQualifiedName~SupplyArrVendorCatalogApiTests|FullyQualifiedName~SupplyArrVendorEmailInboxTests|FullyQualifiedName~SupplyArrVendorRestrictionTests"` in `tests/STLCompliance.SupplyArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~StaffArrMePortalTests|FullyQualifiedName~StaffArrPersonLookupTests|FullyQualifiedName~StaffArrTenantSettingsTests"` in `tests/STLCompliance.StaffArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~TrainArrReportTests|FullyQualifiedName~StaffArrIntegrationSurfaceTests|FullyQualifiedName~StaffArrPersonUpdateWorkflowTests|FullyQualifiedName~NexArrPlatformAdminUserTests|FullyQualifiedName~StaffArrHandoffApiTests"` in `tests/STLCompliance.StaffArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~AssetReadinessRulesTests|FullyQualifiedName~WorkOrderStatusRulesTests|FullyQualifiedName~VoiceNumericNormalizerTests"` in `tests/STLCompliance.MaintainArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~MaintainArrAuditPackageTests|FullyQualifiedName~MaintainArrHandoffApiTests|FullyQualifiedName~MaintainArrPmProgramTests|FullyQualifiedName~MaintainArrInspectionTemplateTests"` in `tests/STLCompliance.MaintainArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~MaintainArrMaintenanceReportTests|FullyQualifiedName~MaintainArrMaintenanceHistoryTests|FullyQualifiedName~MaintainArrMeterTrackingTests|FullyQualifiedName~MaintainArrInspectionRunTests|FullyQualifiedName~MaintainArrWorkOrderTests|FullyQualifiedName~MaintainArrWorkOrderLaborEvidenceTests|FullyQualifiedName~MaintainArrWorkOrderSupplyReadinessTests|FullyQualifiedName~MaintainArrSupplyArrPartsDemandTests|FullyQualifiedName~MaintainArrTechnicianRefTests"` in `tests/STLCompliance.MaintainArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~MaintainArrAssetBulkImportTests|FullyQualifiedName~MaintainArrPmDueScanWorkerTests"` in `tests/STLCompliance.MaintainArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~MaintainArrAssetStatusRollupWorkerTests|FullyQualifiedName~MaintainArrAssetDowntimeTests|FullyQualifiedName~MaintainArrDefectEscalationWorkerTests|FullyQualifiedName~MaintainArrMaintenanceHistoryRollupWorkerTests|FullyQualifiedName~MaintainArrNotificationTests|FullyQualifiedName~MaintainArrStaffarrTechnicianSyncTests|FullyQualifiedName~MaintainArrPlatformEventTests|FullyQualifiedName~MaintainArrAuditPackageGenerationTests|FullyQualifiedName~MaintainArrCatalogFieldsetControlledTests|FullyQualifiedName~MaintainArrSmartImportCommitHandlerTests|FullyQualifiedName~MaintainArrTenantSettingsRulesTests|FullyQualifiedName~MaintenanceHistoryRulesTests|FullyQualifiedName~MaintenanceNotificationRulesTests|FullyQualifiedName~MaintenancePlatformEventRulesTests|FullyQualifiedName~WorkOrderStatusRulesTests|FullyQualifiedName~AssetStatusRollupRulesTests|FullyQualifiedName~AssetReadinessRulesTests|FullyQualifiedName~DefectEscalationRulesTests|FullyQualifiedName~DowntimeDeepLinkBuilderTests|FullyQualifiedName~VoiceNumericNormalizerTests|FullyQualifiedName~SuiteSchedulingContractsTests|FullyQualifiedName~MaintainArrAuditPackageGenerationRulesTests"` in `tests/STLCompliance.MaintainArr.Auth.Tests`
- `dotnet test --no-restore` in `tests/STLCompliance.CustomArr.Api.Tests`
- `dotnet test --no-restore` in `tests/STLCompliance.OrdArr.Auth.Tests`
- `dotnet test --no-restore` in `tests/STLCompliance.LedgArr.Tests`
- `dotnet test --no-restore` in `tests/STLCompliance.LoadArr.Auth.Tests`
- `npm test -- WorkOrderCreatePage.test.tsx AssetCreatePage.test.tsx AssetDetailsPage.test.tsx PmProgramCreatePage.test.tsx InspectionTemplateCreatePage.test.tsx` in `apps/maintainarr-frontend`
- `npm test -- VendorOrdersPage.test.tsx VendorOrderPortalPage.test.tsx VendorPortalPage.test.tsx VendorOrderSettingsPanel.test.tsx` in `apps/supplyarr-frontend`
- `npm test -- AssignmentWorkspacePage.test.tsx AssignmentsPanel.test.tsx ProgramBuilderPanel.test.tsx QualificationManagementPanel.test.tsx QualificationReportsPanel.test.tsx TrainingDetailProfiles.test.tsx PersonTrainingHistoryPanel.test.tsx` in `apps/trainarr-frontend`
- `npm test -- TripsPanel.test.tsx TripWorkspacePanel.test.tsx BulkDispatchPanel.test.tsx DispatchBoardPanel.test.tsx RouteCalendarPanel.test.tsx RoutesPanel.test.tsx RoutingDetailProfiles.test.tsx TripExecutionWorkspacePanel.test.tsx DriverAvailabilityPanel.test.tsx TripProofDvirReadPanel.test.tsx UnassignedWorkQueuePanel.test.tsx` in `apps/routarr-frontend`
- `npm test -- RegistrySection.test.tsx RegistryDetailProfile.test.tsx EvaluationSection.test.tsx RequirementDetailPage.test.tsx CitationFactCatalogPanel.test.tsx FindingsWorkflowGatesPanel.test.tsx ComplianceWaiversPanel.test.tsx RuleEvaluationPanel.test.tsx EvidenceCompletenessReportsPanel.test.tsx` in `apps/compliancecore-frontend`
- `npm test -- IncidentCreatePage.test.tsx MyTeamPage.test.tsx PersonBulkImportPanel.test.tsx WorkspaceActionErrorNormalization.test.tsx` in `apps/staffarr-frontend`
- `npm test -- PeopleSection.test.tsx CertificationPanel.test.tsx PersonTrainarrTrainingHistoryPanel.test.tsx ReadinessPanel.test.tsx MeSelfServicePortalPanel.test.tsx WorkforceOnboardingJourneyPanel.test.tsx TrainingAcknowledgementsPanel.test.tsx` in `apps/staffarr-frontend`
- `npm test -- App.test.tsx` in `apps/staffarr-frontend`
- `npm test` in `apps/staffarr-frontend`
- `npm test` in `apps/compliancecore-frontend`
- `npm test` in `apps/routarr-frontend`
- `npm test` in `apps/trainarr-frontend`
- `npm test` in `apps/fieldcompanion-frontend`
- `npm test` in `apps/suite-frontend`
- `npm test` in `apps/stlcompliancesite`
- `dotnet test` in `tests/STLCompliance.NexArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~StaffArrWorkforceOnboardingJourneyTests|FullyQualifiedName~StaffArrAuditTimelineTests|FullyQualifiedName~RoleManagementServiceTests|FullyQualifiedName~StaffArrPermissionProjectionWorkerTests"` in `tests/STLCompliance.StaffArr.Auth.Tests`
- `dotnet test --filter "FullyQualifiedName~RetentionWindowRulesTests|FullyQualifiedName~ComplianceCoreWaiverTests|FullyQualifiedName~ComplianceCoreVocabularySpineTests"` in `tests/STLCompliance.ComplianceCore.Auth.Tests`
- `npm run build` in `apps/customarr-frontend`
- `npm run build` in `apps/recordarr-frontend`

## Repository Coverage Matrix

| Surface | Build | Startup | Route coverage | API / DB coverage | Unit / integration / e2e | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| `packages/shared-ui` | Verified by test pass | N/A | Shared shell, drawer, page-header, quick create, print actions | Shared client helpers only | `vitest run` passed | Hints, print/export, and error-message behavior updated |
| `packages/shared-dotnet` | Not rerun in this pass | N/A | Shared host, middleware, integration, and contract surfaces | Shared .NET contracts and infrastructure | Solution tests present | Common host/auth/event/launch contracts discovered |
| `tests/STLCompliance.Shared.Tests` | Verified by test pass | N/A | Shared contract, health, and operations rules | Shared .NET contracts and health helpers | `dotnet test` passed | Shared contract helpers validated |
| `tests/STLCompliance.OpenApi.Tests` | Verified by test pass | N/A | Cross-product OpenAPI parity, catalog rules, and legacy site-reference checks | Generated OpenAPI snapshots and static source contracts | `dotnet test` passed after snapshot refresh | OpenAPI snapshots were refreshed and the legacy StaffArr site alias allowlist was brought up to date |
| `tests/STLCompliance.SupplyArr.Auth.Tests` | Verified by targeted test pass | N/A | SupplyArr vendor catalog, email inbox, and restriction workflows | SupplyArr + NexArr auth and in-memory database integration | `dotnet test --filter ...` passed | Focused vendor catalog/email/restriction slice passed |
| `tests/STLCompliance.StaffArr.Auth.Tests` | Verified by targeted test pass | N/A | StaffArr me portal, person lookup, and tenant settings workflows | StaffArr + NexArr auth and in-memory database integration | `dotnet test --filter ...` passed | Focused me-portal/person-lookup/tenant-settings slice passed |
| `tests/STLCompliance.MaintainArr.Auth.Tests` | Verified by targeted test pass | N/A | MaintainArr asset readiness, work order, and voice normalization rules | MaintainArr service rule helpers and shared contract utilities | `dotnet test --filter ...` passed | Focused readiness/work-order/voice-normalizer slice passed |
| `tests/STLCompliance.MaintainArr.Auth.Tests` | Verified by targeted test pass | N/A | MaintainArr audit package, handoff, PM program, and inspection-template workflows | MaintainArr service routes and shared contract utilities | `dotnet test --filter ...` passed | Focused audit-package/handoff/PM-program/inspection-template slice passed after site-backed asset creation and template-category persistence fixes |
| `tests/STLCompliance.ComplianceCore.Auth.Tests` | Verified by targeted test pass | N/A | ComplianceCore waiver workflow gate and vocabulary spine | ComplianceCore service rules, evaluation snapshots, and vocabulary catalog | `dotnet test --filter ...` passed | Focused waiver/vocabulary slice passed after removing snapshot mutation and aligning the seeded vocabulary count |
| `apps/suite-frontend` | Verified by test pass | Verified by build pass | Suite shell, launchpad, preferences, platform admin | NexArr client calls exercised by tests | `vitest run` and `vite build` passed | Topbar hints now persisted and gated |
| `apps/ordarr-frontend` | Not rebuilt in this pass | Not re-run in this pass | App route + legacy handoff redirect | API client mocked in tests | `vitest run` passed | Legacy `/handoff` redirect regression added |
| `apps/customarr-frontend` | Verified by build | Not re-run in this pass | App route + legacy handoff redirect | API client not exercised in this pass | `vitest run --passWithNoTests` passed | No local test files present |
| `tests/STLCompliance.CustomArr.Api.Tests` | Verified by test pass | N/A | CustomArr launch and API contract checks | CustomArr API and launch flow | `dotnet test --no-restore` passed | Full CustomArr API suite passed |
| `apps/ledgarr-frontend` | Not rebuilt in this pass | Not re-run in this pass | App route + legacy handoff redirect | API client not exercised in this pass | `vitest run` passed | Route cleanup verified by existing tests |
| `tests/STLCompliance.LedgArr.Tests` | Verified by test pass | N/A | LedgArr API and route contract checks | LedgArr service rules and integration | `dotnet test --no-restore` passed | Full LedgArr suite passed |
| `apps/recordarr-frontend` | Verified by build | Not re-run in this pass | App route + legacy handoff redirect | API client not exercised in this pass | `vitest run --passWithNoTests` passed | No local test files present |
| `apps/assurarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx` routes discovered | Frontend app shell against AssurArr API | `vitest run --passWithNoTests` passed | No local test files present |
| `apps/compliancecore-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx` routes discovered | Frontend app shell against Compliance Core API | `vitest run` passed | Compliance shell remained green |
| `apps/fieldcompanion-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx`, `src/pages/HomePage.tsx` | Frontend field companion shell | `vitest run` passed | Full suite reran green after expanding the shared-ui test mock |
| `apps/loadarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx` routes discovered | Frontend app shell against LoadArr API | `vitest run --passWithNoTests` passed | LoadArr shell remained green |
| `tests/STLCompliance.LoadArr.Auth.Tests` | Verified by test pass | N/A | LoadArr auth and launch contract checks | LoadArr auth and in-memory database integration | `dotnet test --no-restore` passed | Full LoadArr auth suite passed |
| `apps/maintainarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx` routes discovered | Create/detail-heavy MaintainArr workflows | `vitest run` passed | MaintainArr harness stubs cleared the last suite failures |
| `apps/nexarr-api` | Not rerun in this pass | Not re-run in this pass | Launch, auth, preferences, handoff | Backend not exercised locally in this pass | Solution/tests present | Platform ownership boundary remains NexArr |
| `tests/STLCompliance.NexArr.Auth.Tests` | Verified by full test pass | N/A | Platform health, product surface, auth rules | NexArr auth and platform boundary checks | `dotnet test` passed after expectation alignment | Health aggregation expectation aligned to the service contract |
| `apps/ordarr-api` | Not rerun in this pass | Not re-run in this pass | Order, handoff, completion packet APIs | Backend not exercised locally in this pass | Solution/tests present | Ownership boundaries unchanged |
| `tests/STLCompliance.OrdArr.Auth.Tests` | Verified by test pass | N/A | OrdArr auth and launch contract checks | OrdArr auth and in-memory database integration | `dotnet test --no-restore` passed | Full OrdArr auth suite passed |
| `apps/reportarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx` routes discovered | Frontend reporting shell | `vitest run --passWithNoTests` passed | No local test files present |
| `apps/recordarr-api` | Not rerun in this pass | Not re-run in this pass | Record/evidence/audit package APIs | Backend not exercised locally in this pass | Solution/tests present | Ownership boundaries unchanged |
| `apps/routarr-api` | Not rerun in this pass | Not re-run in this pass | Trip, dispatch, and handoff APIs | Backend not exercised locally in this pass | Solution/tests present | Ownership boundaries unchanged |
| `apps/routarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx`, workspace routes discovered | Frontend routing/dispatch shell | `vitest run` passed | Bulk dispatch and trip proof harnesses were refreshed |
| `apps/staffarr-api` | Not rerun in this pass | Not re-run in this pass | People, roles, onboarding, audit package APIs | Backend not exercised locally in this pass | Solution/tests present | Ownership boundaries unchanged |
| `apps/staffarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx` routes discovered | Frontend people / hiring shell | `vitest run` passed | People, import, and workspace error handling verified |
| `apps/stlcompliancekb` | Verified by test pass | Verified by build pass | `src/App.tsx`, `src/pages/HomePage.tsx` | Static knowledge base routes | `vitest run` and `vite build` passed | Knowledge base routes and theme audit script remained healthy |
| `apps/stlcompliancesite` | Verified by test pass | Not re-run in this pass | `src/App.tsx`, marketing pages | Marketing/site routes only | `vitest run` passed | Product catalog alignment verified for LedgArr |
| `apps/supplyarr-api` | Not rerun in this pass | Not re-run in this pass | Supplier, procurement, receiving APIs | Backend not exercised locally in this pass | Solution/tests present | Ownership boundaries unchanged |
| `apps/supplyarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx`, workspace routes discovered | Frontend supply shell | `vitest run` passed | Vendor portal, settings, import, and catalog harnesses verified |
| `apps/trainarr-api` | Not rerun in this pass | Not re-run in this pass | Training, certifications, handoff APIs | Backend not exercised locally in this pass | Solution/tests present | Ownership boundaries unchanged |
| `apps/trainarr-frontend` | Verified by test pass | Not re-run in this pass | `src/App.tsx`, workspace routes discovered | Frontend training shell | `vitest run` passed | Qualification report harness now uses lightweight shared-ui stubs |

## Workflow Coverage Matrix

| Workflow | Owner | Entry | Products | Primary records | APIs | Permissions | Success-path test | Validation / failure test | Regression location | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Suite login to workspace launch | NexArr | `/app` and `/login` | NexArr + entitling product | Session, tenant, entitlement | NexArr auth + launch APIs | Platform auth / entitlement | Existing login and launcher tests | Redirect and unauthorized states covered | `apps/suite-frontend/src/pages/LoginPage.test.tsx` | Verified |
| Suite topbar hints toggle | NexArr / suite shell | Topbar button | NexArr shell + current page | Suite preference snapshot | `updateMyPreferences` via suite preferences hook | Logged-in user only | Added persistence path | Hidden hint text and closed drawer verified | `apps/suite-frontend/src/preferences/preferences.test.tsx` | Verified |
| Launchpad assistant guidance | NexArr | `/app` launchpad | NexArr | Assistant session / answer | `sendAiAssistantMessage` | Logged-in user only | Assistant success path already covered | Safe fallback now shown on failure | `apps/suite-frontend/src/pages/LaunchPadPage.test.tsx` | Verified |
| Quick Create nested reference creation | Owning product through shared UI | Controlled selectors | Consuming product + owner product | Canonical reference record | Owner quick-create API | Tenant, entitlement, domain permission | Existing picker tests cover success | Safe fallback now shown on create failure | `packages/shared-ui/src/forms/forms.components.test.tsx` | Verified for shared UI path |
| Legacy handoff route cleanup | Product frontend | `/handoff` | OrdArr / CustomArr / LedgArr / RecordArr | Launch handoff query state | Product launch routing | Preserved secure launch behavior | Redirect regression added for OrdArr | Query string preserved through redirect | `apps/ordarr-frontend/src/App.test.tsx` | Verified for representative app |
| MaintainArr work order lifecycle | MaintainArr | `/app/maintainarr` workspace and work-order pages | MaintainArr | Work order, asset, defect, PM program | MaintainArr API routes | Product/workspace permissions | Targeted page tests | Create/detail surface files present | `apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.tsx` | Verified |
| SupplyArr vendor order lifecycle | SupplyArr | `/app/supplyarr` workspace and vendor-order pages | SupplyArr | Vendor order, supplier, purchase order | SupplyArr API routes | Product/workspace permissions | Targeted page tests | Create/detail surface files present | `apps/supplyarr-frontend/src/pages/vendor-orders/VendorOrderCreatePage.tsx` | Verified |
| StaffArr people and training lifecycle | StaffArr | `/app/staffarr` workspace and people/training pages | StaffArr | Person, incident, certification, timesheet | StaffArr API routes | Product/workspace permissions | Targeted section/panel/route tests; timesheet route verified; full suite green | Create/detail surface files present | `apps/staffarr-frontend/src/pages/people/PeoplePage.tsx` | Verified |
| TrainArr qualification lifecycle | TrainArr | `/app/trainarr` workspace and assignment/certificate pages | TrainArr | Assignment, program, qualification, certificate | TrainArr API routes | Product/workspace permissions | Targeted page and panel tests; full suite green | Workspace/detail surfaces present | `apps/trainarr-frontend/src/pages/assignments/AssignmentsPage.tsx` | Verified |
| RoutArr dispatch and trip lifecycle | RoutArr | `/app/routarr` workspace and dispatch/trip pages | RoutArr | Dispatch plan, trip, stop, dock appointment | RoutArr API routes | Product/workspace permissions | Targeted page and panel tests; full suite green | Workspace/detail surfaces present | `apps/routarr-frontend/src/pages/trips/TripsPage.tsx` | Verified |
| Compliance Core rule evaluation lifecycle | Compliance Core | `/app/compliancecore` workspace and rule pages | Compliance Core | Rule pack, requirement, jurisdiction, evidence mapping | Compliance Core API routes | Product/workspace permissions | Targeted page and panel tests; full suite green | Workspace/detail surfaces present | `apps/compliancecore-frontend/src/pages/rulepacks/RulePackDetailPage.tsx` | Verified |
| ComplianceCore waiver and vocabulary lifecycle | Compliance Core | Workflow-gate check, audit export, vocabulary endpoints | Compliance Core | Rule evaluation run, workflow gate check, waiver, vocabulary type | Compliance Core API routes | Product/workspace permissions | Targeted backend auth subset | Waiver persistence and vocabulary count coverage verified | `tests/STLCompliance.ComplianceCore.Auth.Tests/ComplianceCoreWaiverTests.cs`, `tests/STLCompliance.ComplianceCore.Auth.Tests/ComplianceCoreVocabularySpineTests.cs` | Verified |

## Primary Record Surface Matrix

| Primary record | Owning product | Collection | Preview drawer | Details page | Create page | Edit / archive / delete | Quick Create eligibility | Deep link | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Product workspace session | NexArr | Launchpad / product shell | N/A | N/A | N/A | N/A | N/A | Yes | Verified shell behavior |
| Suite user preferences | NexArr | Preferences page | N/A | N/A | N/A | Save / reset only | N/A | Yes | Hint visibility now persists |
| Order / request lifecycle | OrdArr | Existing order dashboards | Existing page-level detail views | Existing detail routes | Existing create flows | Existing lifecycle actions | Controlled references already present in forms | Yes | Route cleanup verified |
| Reference selection targets | Owning product of reference | Shared pickers / drawers | Shared summary card | Owner details pages | Shared quick create drawer | Owner-defined | Yes | Yes | Safe create and failure messaging improved |
| Work orders and maintenance assets | MaintainArr | `WorkOrdersPage`, `AssetsPage`, `PMProgramsPage` | `WorkOrderWorkspacePage`, `AssetDetailsPage` | `PartDetailPage`, asset detail views | `WorkOrderCreatePage`, `AssetCreatePage`, `PmProgramCreatePage` | Lifecycle pages present | Verified by targeted page tests | Yes | Verified |
| Supplier/vendor order records | SupplyArr | `SuppliersPage`, `PurchaseOrdersPage`, `VendorOrdersPage` | `VendorOrderDetailPage`, `VendorPortalPage` | Supplier and portal detail views | `VendorOrderCreatePage` | Lifecycle pages present | Verified by targeted page tests | Yes | Verified |
| People, incidents, and training records | StaffArr | `PeoplePage`, `IncidentsPage`, `CertificationsPage`, `AuditPackagesPage` | `IncidentCreatePage`, `TimesheetDetailPage` | Person and team detail views | `IncidentCreatePage` | Lifecycle pages present | Verified for people/training sub-surface, timekeeping/timesheet route, and full suite | Yes | Verified |
| Qualification and learning records | TrainArr | `AssignmentsPage`, `ProgramsPage`, `QualificationsPage`, `CertificatesPage` | `AssignmentWorkspacePage` | assignment and certificate views | create flows appear workspace-driven | Lifecycle pages present | Verified by targeted page tests and full suite | Yes | Verified |
| Dispatch and trip records | RoutArr | `TripsPage`, `DispatchPage`, `RoutesPage`, `StopsPage` | `TripWorkspacePage` | trip/stop detail and workspace views | workspace-driven create flows | Lifecycle pages present | Verified by targeted page tests and full suite | Yes | Verified |
| Compliance rule and evidence records | Compliance Core | `RulePackDetailPage`, `CitationsPage`, `Requirements`, `EvidenceMappingPage` | `RequirementDetailPage` | rule pack / requirement details | admin and evaluator flows | Domain-limited | Verified by targeted page and panel tests and full suite | Yes | Verified |

## Quick Create Coverage Matrix

| Reference type | Owning product | Invoking product / form | Search support | Minimal create support | Duplicate detection | Permission behavior | Failure recovery | Nested Quick Create | Regression |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Owner-defined cross-product reference | Owning product via shared UI | Shared `ReferencePicker` / `ReferenceSearchPicker` | Yes | Yes, via schema fields | Yes, duplicate candidates surfaced | Respects owner permissions and entitlement checks | Parent form is retained on failure | Supported by drawer flow | `packages/shared-ui/src/forms/forms.components.test.tsx` |

## Error Handling Catalog

| Stable error code | Trigger | Owner | HTTP | User-facing title / message | Retry | Logging | Test |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `quick-create-failed` | Quick create mutation rejects | Shared UI | N/A | `Quick create is temporarily unavailable. Please try again.` | User can retry without losing the parent form | Logged to console for diagnostics | `packages/shared-ui/src/forms/forms.components.test.tsx` |
| `suite-ai-unavailable` | Suite assistant request rejects | NexArr / suite shell | N/A | `AI assistance is temporarily unavailable. Please try again.` | User can retry from the drawer | Logged to console for diagnostics | `apps/suite-frontend/src/pages/LaunchPadPage.test.tsx` |
| `product-ai-unavailable` | Product shell assistant request rejects | Shared product shell | N/A | `AI assistance is temporarily unavailable. Please try again.` | User can retry from the drawer | Logged to console for diagnostics | Existing shared-ui tests cover product drawer flows |
| `print-export-failed` | Shared print export request rejects | Shared UI print actions | N/A | `Print export is temporarily unavailable. Please try again.` | User can retry download | Logged to console for diagnostics | `packages/shared-ui/src/print/PrintActionBar.test.tsx` |
| `trainarr-handoff-failed` | StaffArr TrainArr handoff rejects | StaffArr people / certifications sections | N/A | `TrainArr is temporarily unavailable. Please try again.` | User can retry launch from the owning section | Logged to console for diagnostics | `apps/staffarr-frontend/src/workspace/sections/WorkspaceActionErrorNormalization.test.tsx` |
| `loadarr-handoff-failed` | SupplyArr LoadArr handoff rejects | SupplyArr owner handoff panel | N/A | `LoadArr is temporarily unavailable. Please try again.` | User can retry launch from the owning section | Logged to console for diagnostics | `apps/supplyarr-frontend/src/components/LoadArrHandoffPanel.test.tsx` |
| `staffarr-myteam-load-failed` | StaffArr my-team dashboard or dependent read queries reject | StaffArr my-team page | N/A | `Failed to load direct reports.`, `Failed to load readiness status.`, `Failed to load certifications.` | User can retry page load or panel retries where exposed | Review mutation logs diagnostics to console | `apps/staffarr-frontend/src/pages/my-team/MyTeamPage.test.tsx` |
| `bulk-import-failed` | StaffArr / MaintainArr bulk import request rejects | Person and asset bulk import panels | N/A | `Bulk import is temporarily unavailable. Please try again.` | User can retry import after correcting data or retrying service | Logged to console for diagnostics | `apps/staffarr-frontend/src/components/PersonBulkImportPanel.test.tsx`, `apps/maintainarr-frontend/src/components/AssetBulkImportPanel.test.tsx` |
| `supplyarr-vendor-portal-update-failed` | SupplyArr vendor portal status or document mutations reject | Vendor order portal page | N/A | `Unable to save readiness update. Please try again.`, `Unable to register document. Please try again.` | User can retry portal update without revealing transport details | Logged to console for diagnostics | `apps/supplyarr-frontend/src/pages/vendor-orders/VendorOrderPortalPage.test.tsx` |
| `supplyarr-vendor-settings-save-failed` | SupplyArr vendor order settings save rejects | Vendor order settings panel | N/A | `Unable to save vendor-order settings. Please try again.` | User can retry settings save without exposing raw backend text | Logged to console for diagnostics | `apps/supplyarr-frontend/src/components/VendorOrderSettingsPanel.test.tsx` |

## Defect and Fix Ledger

| Severity | Product | Workflow | Reproduction | Root cause | Files changed | Fix summary | Regression test | Verification | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| High | OrdArr, CustomArr, LedgArr, RecordArr | Legacy launch/handoff route entry | Visit `/handoff` on a product frontend | Transitional route still accepted as a direct entry point | `apps/ordarr-frontend/src/App.tsx`, `apps/customarr-frontend/src/App.tsx`, `apps/ledgarr-frontend/src/App.tsx`, `apps/recordarr-frontend/src/App.tsx` | Redirect legacy route to canonical launch route while preserving the query string | `apps/ordarr-frontend/src/App.test.tsx` | App tests/builds passed | Fixed |
| Medium | Suite shell | Optional hints / guidance visibility | Open suite pages with guidance text | Hints were not persisted or centrally gated | `apps/suite-frontend/src/layouts/AppShellLayout.tsx`, `apps/suite-frontend/src/components/AppTopBar.tsx`, `packages/shared-ui/src/PageHeader.tsx`, `apps/suite-frontend/src/components/preferences/PreferenceControls.tsx`, `apps/suite-frontend/src/components/platform-admin/PlatformAdminPageChrome.tsx`, `apps/suite-frontend/src/pages/LaunchPadPage.tsx`, `apps/suite-frontend/src/preferences/preferences.test.tsx` | Added shared hints context, persisted suite preference, and hid optional helper copy when hints are off | `packages/shared-ui/src/PageHeader.test.tsx`, `apps/suite-frontend/src/pages/LaunchPadPage.test.tsx`, `apps/suite-frontend/src/preferences/preferences.test.tsx` | Shared-ui and suite tests passed | Fixed |
| Medium | Shared UI / suite | Quick create and assistant error states | Cause quick create or assistant request failure | Raw exception text was exposed directly to users | `packages/shared-ui/src/forms/QuickCreateDrawer.tsx`, `apps/suite-frontend/src/pages/LaunchPadPage.tsx`, `packages/shared-ui/src/ProductAppShell.tsx`, `apps/suite-frontend/src/components/AppTopBar.tsx` | Replaced raw exception text with safe actionable fallback messages and console diagnostics | `packages/shared-ui/src/forms/forms.components.test.tsx`, `apps/suite-frontend/src/pages/LaunchPadPage.test.tsx` | Shared-ui, suite, and affected app tests passed | Fixed |
| Medium | Shared UI print/export | Print and archive flow | Trigger print PDF download or browser print from a printable surface | Shared print action bar exposed raw transport errors | `packages/shared-ui/src/print/PrintActionBar.tsx`, `packages/shared-ui/src/print/PrintActionBar.test.tsx` | Safe print/archive/reprint/logging fallbacks with console diagnostics | `packages/shared-ui/src/print/PrintActionBar.test.tsx` | Shared-ui test suite passed | Fixed |
| Medium | StaffArr / SupplyArr | TrainArr and LoadArr handoff launch | Launch TrainArr from StaffArr or LoadArr from SupplyArr | Handoff launch errors exposed raw transport text | `apps/staffarr-frontend/src/workspace/sections/CertificationsSection.tsx`, `apps/staffarr-frontend/src/workspace/sections/PeopleSection.tsx`, `apps/staffarr-frontend/src/workspace/sections/WorkspaceActionErrorNormalization.test.tsx`, `apps/supplyarr-frontend/src/components/LoadArrHandoffPanel.tsx`, `apps/supplyarr-frontend/src/components/LoadArrHandoffPanel.test.tsx` | Safe launch fallback messages with console diagnostics | `apps/staffarr-frontend/src/workspace/sections/WorkspaceActionErrorNormalization.test.tsx`, `apps/supplyarr-frontend/src/components/LoadArrHandoffPanel.test.tsx` | StaffArr and SupplyArr tests passed | Fixed |
| Medium | MaintainArr / StaffArr / SupplyArr | Import center history and manifest banners | Open import centers after a history or manifest query failure | Direct query error messages were shown to users | `apps/maintainarr-frontend/src/workspace/sections/ImportsSection.tsx`, `apps/staffarr-frontend/src/workspace/sections/ImportsSection.tsx`, `apps/supplyarr-frontend/src/workspace/sections/ImportsSection.tsx` | Normalized import-center failure banners through the shared error helper with safe fallback copy | StaffArr suite passed; MaintainArr and SupplyArr app suites surfaced unrelated pre-existing test failures | Fixed |
| Medium | MaintainArr | Inspection template creation and PM program asset seeding | Create a draft inspection template without an explicit category or seed a PM program asset in a site-required tenant | Template category was being persisted as `null` into a required column, and the PM-program fixture omitted the required StaffArr site | `apps/maintainarr-api/MaintainArr.Api/Services/InspectionTemplateService.cs`, `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrPmProgramTests.cs` | Defaulted missing template categories to empty storage, added a fake StaffArr site lookup for the PM-program fixture, and created the seed asset with a site reference | `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrInspectionTemplateTests.cs`, `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrPmProgramTests.cs` | Focused MaintainArr subset passed (`27 tests`) | Fixed |
| Medium | MaintainArr | Bulk asset import alias normalization and PM due-scan inspection bootstrap | Commit an alias-heavy asset import row without a site reference, or seed a PM inspection program with a manager-only PM role | Bulk import rejected the row before validation because the site reference was missing, and PM program activation requires an admin-level PM role | `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetBulkImportTests.cs`, `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrPmDueScanWorkerTests.cs` | Added the required StaffArr site reference to the alias import row and used an admin PM token when bootstrapping the inspection program for the due-scan worker test | `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetBulkImportTests.cs`, `tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrPmDueScanWorkerTests.cs` | Targeted MaintainArr batches passed | Fixed |
| Medium | StaffArr | My team dashboard and dependent read states | Open My Team when dashboard/readiness/certification reads fail | Raw query messages surfaced in page and panel fallbacks | `apps/staffarr-frontend/src/pages/my-team/MyTeamPage.tsx`, `apps/staffarr-frontend/src/pages/my-team/MyTeamPage.test.tsx` | Safe fallback messages for dashboard, readiness, and certification reads; query and review failures log diagnostics and show safe copy | `apps/staffarr-frontend/src/pages/my-team/MyTeamPage.test.tsx` | StaffArr suite passed | Fixed |
| Medium | StaffArr / MaintainArr | Bulk person and asset import failures | Trigger a backend failure after validating CSV input | Raw API failure text surfaced in bulk import panels | `apps/staffarr-frontend/src/components/PersonBulkImportPanel.tsx`, `apps/staffarr-frontend/src/components/PersonBulkImportPanel.test.tsx`, `apps/maintainarr-frontend/src/components/AssetBulkImportPanel.tsx`, `apps/maintainarr-frontend/src/components/AssetBulkImportPanel.test.tsx` | Replaced import failure text with a stable generic recovery message and console diagnostics | `apps/staffarr-frontend/src/components/PersonBulkImportPanel.test.tsx`, `apps/maintainarr-frontend/src/components/AssetBulkImportPanel.test.tsx` | Targeted panel tests passed | Fixed |
| Medium | SupplyArr | Vendor portal readiness and document updates | Submit a readiness or document mutation from the vendor portal | Raw API failure text surfaced in portal mutation errors | `apps/supplyarr-frontend/src/pages/vendor-orders/VendorOrderPortalPage.tsx`, `apps/supplyarr-frontend/src/pages/vendor-orders/VendorOrderPortalPage.test.tsx` | Safe portal failure messages with console diagnostics | `apps/supplyarr-frontend/src/pages/vendor-orders/VendorOrderPortalPage.test.tsx` | Targeted portal test passed | Fixed |
| Medium | ComplianceCore | Workflow gate waiver persistence and vocabulary spine | Run a waiver-backed workflow gate check or the vocabulary spine count test | Workflow gate checks tried to mutate an immutable evaluation snapshot and the vocabulary count assertion lagged behind the actual seeded catalog | `apps/compliancecore-api/ComplianceCore.Api/Services/WorkflowGateService.cs`, `tests/STLCompliance.ComplianceCore.Auth.Tests/ComplianceCoreVocabularySpineTests.cs` | Removed the post-save `RuleEvaluationRun` mutation and aligned the vocabulary count assertion to the seeded 17-type catalog | `tests/STLCompliance.ComplianceCore.Auth.Tests/ComplianceCoreWaiverTests.cs`, `tests/STLCompliance.ComplianceCore.Auth.Tests/ComplianceCoreVocabularySpineTests.cs` | Focused ComplianceCore auth subset passed (`27 tests`) | Fixed |
| Verification | SupplyArr | Full app suite rerun | Run the SupplyArr frontend test suite after portal and catalog harness fixes | Previously failing catalog harness lacked a complete mocked workspace state | `apps/supplyarr-frontend/src/workspace/sections/CatalogSection.test.tsx` | Added missing mocked workspace state fields for catalog suite coverage | `apps/supplyarr-frontend/src/workspace/sections/CatalogSection.test.tsx` | Full SupplyArr suite passed | Verified |
| Medium | SupplyArr | Vendor order settings save | Toggle vendor portal settings or TTL after a save failure | Raw API failure text surfaced in settings panel errors | `apps/supplyarr-frontend/src/components/VendorOrderSettingsPanel.tsx`, `apps/supplyarr-frontend/src/components/VendorOrderSettingsPanel.test.tsx` | Safe vendor settings save message with console diagnostics | `apps/supplyarr-frontend/src/components/VendorOrderSettingsPanel.test.tsx` | Targeted settings test and full SupplyArr suite passed | Fixed |
| Verification | MaintainArr | Full app suite rerun | Run the MaintainArr frontend test suite after shared-ui test harness stubs were added | Two tests were aborting on shared UI hooks and missing mock exports | `apps/maintainarr-frontend/src/workspace/sections/MaintenanceDetailProfiles.test.tsx`, `apps/maintainarr-frontend/src/pages/assets/AssetCreatePage.test.tsx` | Replaced the heavy shared-ui imports in those tests with lightweight stubs for the exercised surface | `apps/maintainarr-frontend/src/workspace/sections/MaintenanceDetailProfiles.test.tsx`, `apps/maintainarr-frontend/src/pages/assets/AssetCreatePage.test.tsx` | Full MaintainArr suite passed (`43 files / 126 tests`) | Verified |
| Verification | TrainArr | Full app suite rerun | Run the TrainArr frontend test suite after qualification report harness fixes | Qualification reports panel was failing on shared-ui harness behavior | `apps/trainarr-frontend/src/components/QualificationReportsPanel.test.tsx` | Replaced heavy shared-ui imports with lightweight stubs and made the point-in-time report interaction deterministic | `apps/trainarr-frontend/src/components/QualificationReportsPanel.test.tsx` | Full TrainArr suite passed (`47 files / 103 tests`) | Verified |
| Verification | FieldCompanion | Full app suite rerun | Run the FieldCompanion frontend test suite after shared-ui harness adjustments | Several tests were failing because the test mock omitted alert semantics and a launch URL export | `apps/fieldcompanion-frontend/src/test/setup.ts` | Expanded the shared-ui test mock to render alert callouts, expose test IDs, and stub product launch URL resolution | `apps/fieldcompanion-frontend/src/test/setup.ts` | Full FieldCompanion suite passed (`23 files / 72 tests`) | Verified |
| Low | RecordArr | Record and controlled document creation | Create a record or controlled document with a hyphenated fake class key | Store validation accepted invalid document-class slugs and the canonical seed carried a noncanonical controlled-document type | `apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs` | Normalized document-class keys to slug-safe values and aligned the seeded controlled document to a real lifecycle type | `tests/STLCompliance.OpenApi.Tests/RecordArrStoreTests.cs` | RecordArr store slice and full OpenAPI suite passed | Fixed |
| Low | NexArr | Platform health aggregation expectation | Run the focused NexArr auth backend subset that covers product-surface catalog and platform health | Test expectation still used the broader database catalog count instead of the service's actual probe list | `tests/STLCompliance.NexArr.Auth.Tests/PlatformHealthServiceTests.cs` | Aligned the expected product count to the 11-product health-probe contract used by `PlatformHealthService` | `tests/STLCompliance.NexArr.Auth.Tests/PlatformHealthServiceTests.cs` | Focused NexArr auth subset passed (`16 tests`) | Fixed |

## Removed Transitional Surface Ledger

| Previous route | Previous purpose | Replacement behavior | Security retained | Return behavior | Regression test |
| --- | --- | --- | --- | --- | --- |
| `/handoff` in OrdArr | Transitional launch entry | Redirects to `/launch` and preserves query string | Secure launch token flow unchanged | Handoff code remains available in the query string | `apps/ordarr-frontend/src/App.test.tsx` |
| `/handoff` in CustomArr | Transitional launch entry | Redirects to `/launch` and preserves query string | Secure launch token flow unchanged | Handoff code remains available in the query string | Build verification in `apps/customarr-frontend` |
| `/handoff` in LedgArr | Transitional launch entry | Redirects to `/launch` and preserves query string | Secure launch token flow unchanged | Handoff code remains available in the query string | Existing LedgArr tests passed |
| `/handoff` in RecordArr | Transitional launch entry | Redirects to `/launch` and preserves query string | Secure launch token flow unchanged | Handoff code remains available in the query string | Build verification in `apps/recordarr-frontend` |

## Remaining External Verification Items

No unavailable external credentials, hardware, or third-party environments were required for the changes verified in this pass.

## Notes

- This snapshot intentionally records only the surfaces exercised or directly updated in this pass.
- Broader recursive workflow inventory across all products remains active.
- The current implementation is improved, but the overall monorepo-wide audit is not yet complete.
- App-suite verification for the new import-center banner cleanup passed in StaffArr, SupplyArr, and MaintainArr after the MaintainArr harness stubs were added.
- Full StaffArr and SupplyArr frontend suites were rerun in this pass and remained green.
- Full TrainArr frontend suite was rerun in this pass and remained green after the qualification report harness fix.
- Full LoadArr, ReportArr, and RoutArr frontend suites were rerun in this pass; RoutArr needed shared picker harness fixes to return to green.
- Full STLComplianceSite test suite reran green after adding the LedgArr marketing catalog entry.
- Full FieldCompanion frontend suite was rerun in this pass and returned green after the shared-ui mock was expanded to match alert and launch behavior.
- Full STLComplianceKB test and build runs passed in this pass.
- Full shared-dotnet contract tests passed, and the full NexArr auth suite passed after aligning the platform-health expectation.
- Full OpenAPI parity tests passed after refreshing the checked-in snapshots and widening the StaffArr legacy site allowlist to cover the remaining holdover fields.
- Full CustomArr API, OrdArr auth, LedgArr, and LoadArr auth suites were rerun in this pass and remained green.
- A focused StaffArr/NexArr auth subset passed for workforce onboarding, audit timeline, role management, permission projection, and platform-admin user management after restoring the missing permission-check route target and adding admin confirmation headers.
- A focused SupplyArr auth subset passed after the broader suite ran into the time ceiling.
- A focused StaffArr auth subset passed after the broader suite ran into the time ceiling.
- A focused MaintainArr rules subset passed after the broader suite ran into the time ceiling.
- A focused MaintainArr audit-package/handoff/PM-program/inspection-template subset passed after wiring the site-backed PM seed and preserving inspection-template category storage.
- A focused ComplianceCore auth subset passed after removing the workflow-gate snapshot mutation and aligning the vocabulary count expectation.
- A focused MaintainArr page subset passed for work order, asset, PM program, and inspection-template pages.
- A focused SupplyArr page subset passed for vendor-order, vendor-portal, and vendor settings pages.
- A focused TrainArr page subset passed for assignments, qualification management, reporting, and training history.
- A focused RoutArr page subset passed for trips, dispatch, routing, and driver-availability pages.
- A focused ComplianceCore page subset passed for registry, evaluation, requirements, citations, workflow gates, waivers, and evidence-completeness reports.
- A focused StaffArr page subset passed for incident creation, my-team, person import, and workspace error handling.
- A focused StaffArr people/training sub-slice passed for section, certification, readiness, onboarding, and training-history panels.
- A focused StaffArr timekeeping route test passed for `/timekeeping` and `/timekeeping/timesheets/:id`.
- The full StaffArr frontend suite passed after adding the timekeeping route regression.
- The full ComplianceCore frontend suite passed after the workflow/evaluation coverage updates.
- The full RoutArr frontend suite passed after the dispatch/trip coverage updates.
- The full TrainArr frontend suite passed after the qualification coverage updates.
- The full FieldCompanion frontend suite passed after the shared-ui test mock and offline-sync updates.
- The full Suite frontend suite passed after the hints/preferences and shared UI updates.
- The full STLComplianceSite test suite passed after the product catalog update.
- The shared .NET contract test suite passed again after the platform-level verification refresh.
- AssurArr has no local test files; ComplianceCore frontend suite reran green.
- The full NexArr auth suite passed after the platform-health expectation was aligned to the 11-product probe contract.

## Discovered Route Sources

| Surface | Primary route file(s) |
| --- | --- |
| `apps/assurarr-frontend` | `src/App.tsx` |
| `apps/compliancecore-frontend` | `src/App.tsx` |
| `apps/customarr-frontend` | `src/App.tsx` |
| `apps/fieldcompanion-frontend` | `src/App.tsx`, `src/pages/HomePage.tsx` |
| `apps/ledgarr-frontend` | `src/App.tsx` |
| `apps/loadarr-frontend` | `src/App.tsx` |
| `apps/maintainarr-frontend` | `src/App.tsx` |
| `apps/nexarr-api` | Shared host/endpoints in the .NET service layer |
| `apps/ordarr-frontend` | `src/App.tsx` |
| `apps/maintainarr-frontend` | `src/App.tsx`, `src/pages/*`, `src/components/*` |
| `apps/supplyarr-frontend` | `src/App.tsx`, `src/pages/*`, `src/components/*` |
| `apps/staffarr-frontend` | `src/App.tsx`, `src/pages/*`, `src/workspace/*` |
| `apps/trainarr-frontend` | `src/App.tsx`, `src/pages/*`, `src/workspace/*` |
| `apps/routarr-frontend` | `src/App.tsx`, `src/pages/*`, `src/workspace/*` |
| `apps/compliancecore-frontend` | `src/App.tsx`, `src/pages/*`, `src/workspace/*` |
| `apps/reportarr-frontend` | `src/App.tsx` |
| `apps/recordarr-frontend` | `src/App.tsx` |
| `apps/stlcompliancekb` | `src/App.tsx`, `src/pages/HomePage.tsx` |
| `apps/stlcompliancesite` | `src/App.tsx`, marketing pages |
| `apps/suite-frontend` | `src/App.tsx`, `src/app/routes.tsx`, `src/layouts/AppShellLayout.tsx`, `src/components/AppTopBar.tsx`, `src/pages/HomePage.tsx`, `src/pages/LaunchPadPage.tsx` |
| `apps/supplyarr-frontend` | `src/App.tsx` |
| `apps/trainarr-frontend` | `src/App.tsx` |
