# StaffArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `staffarr` |
| Category | HRM / people, roles, locations |
| Entry release | R1 — Foundation spine |
| Completion release | R4 — Training and qualification gate |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | People, roles, permissions context, org structure, sites, locations, incidents, and delegated account workflows. |
| Roadmap slice | Foundation spine and qualification gate |
| Must not violate | Remain the shared people/location authority while product actions stay owned by the product performing them. |
| Feature rows retained | 72 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R1 | Foundation spine | 16 | 14 |
| R4 | Training and qualification gate | 21 | 1 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R1 unless they are only supporting another release gate.
- Common category baseline remains retained for R4.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## R0 Trust Gate pass

Status: Clear for R0 after focused trust-gate hardening.

StaffArr no longer exposes retired product-access truth in its session bootstrap and `/api/me` contracts. StaffArr-target handoffs now redeem from the authenticated NexArr handoff context and normalize the local StaffArr session to the fixed ordinary-suite launch catalog, while Compliance Core studio remains outside the ordinary product list. Product actions remain server-side permission-scoped through StaffArr authorization.

Files touched:

- `apps/staffarr-api/StaffArr.Api/Contracts/AuthContracts.cs`
- `apps/staffarr-api/StaffArr.Api/Services/HandoffAuthService.cs`
- `apps/staffarr-api/StaffArr.Api/Services/MeService.cs`
- `apps/staffarr-api/StaffArr.Api/Services/StaffArrAuthorizationService.cs`
- `apps/staffarr-api/StaffArr.Api/Services/StaffArrSuiteLaunchCatalog.cs`
- `apps/staffarr-api/StaffArr.Api/Endpoints/OffboardingEndpoints.cs`
- `apps/staffarr-api/StaffArr.Api/Endpoints/PeopleEndpoints.cs`
- `apps/staffarr-api/StaffArr.Api/Endpoints/PersonnelUpdateRequestEndpoints.cs`
- `apps/staffarr-api/StaffArr.Api/Endpoints/V1FeatureAliasEndpoints.cs`
- `apps/staffarr-frontend/src/api/types.ts`
- `apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.test.tsx`
- `apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx`
- `tests/STLCompliance.StaffArr.Auth.Tests/StaffArrHandoffApiTests.cs`
- `tests/STLCompliance.StaffArr.Auth.Tests/StaffArrPersonAccountAccessTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.StaffArr.Auth.Tests.StaffArrHandoffApiTests.Handoff_redeem_succeeds_after_legacy_product_access_revocation" --logger "console;verbosity=normal"`
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName=STLCompliance.StaffArr.Auth.Tests.StaffArrHandoffApiTests.Handoff_redeem_happy_path_returns_session_and_me_works|FullyQualifiedName=STLCompliance.StaffArr.Auth.Tests.StaffArrHandoffApiTests.V1_handoff_session_and_me_aliases_work|FullyQualifiedName=STLCompliance.StaffArr.Auth.Tests.StaffArrHandoffApiTests.Me_allows_users_after_non_staffarr_launch_context|FullyQualifiedName=STLCompliance.StaffArr.Auth.Tests.StaffArrHandoffApiTests.Session_bootstrap_returns_claim_backed_identity" --logger "console;verbosity=minimal"`
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrPersonAccountAccessTests" --logger "console;verbosity=normal"`
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrHandoffApiTests" --logger "console;verbosity=minimal"` — passed, 56 tests in 7m 16s.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --logger "console;verbosity=minimal"` — timed out after 20 minutes in the current repo state.
- `npm test -- ProductWorkspaceLayout.test.tsx MeSelfServicePortalPanel.test.tsx` from `apps/staffarr-frontend`

Remaining blockers:

- Deferred R0 blocker: the full `STLCompliance.StaffArr.Auth.Tests` project still does not complete in the current repo state within a 20-minute local `dotnet test --no-build` window, so StaffArr R0 remains documented against focused trust-gate coverage rather than a clean full-project pass.
- `StaffArrHandoffApiTests` is no longer the blocker by itself; a full class-level run now passes locally (`56` tests in `7m 16s`), which narrows the remaining runtime investigation to the broader StaffArr auth suite.

## R1 Foundation spine pass

Status: Clear for R1 with no StaffArr code changes required in this pass.

Pass notes:

- StaffArr R1 authority rows were reviewed against the product feature/workflow catalogs and ownership constitution. The represented R1 slice is durable people, org/location, role/permission, readiness mirror, incidents, personnel notes/documents, offboarding, update requests, recruiting/application intake, timekeeping, labor evidence, performance, benefits/compensation, exports/audit, tenant settings, and worker-admin behavior.
- Anonymous StaffArr routes are limited to handoff/session exchange and public employment application intake; public employment application access is token-scoped to published, non-expired templates and persists submissions under the owning template tenant.
- StaffArr remains the people/location/role authority while NexArr owns credentials and sessions, TrainArr owns training truth, RecordArr owns stored documents, and ReportArr owns reporting projections.
- `ST-WF-014` qualification-aware staffing remains explicitly partial in the R1 roadmap. It was not expanded into R4 qualification-gate work during this pass.

Files touched:

- `docs/roadmap/products/staffarr.md`

Tests run:

- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrPersonBulkImportTests|FullyQualifiedName~StaffArrPersonLookupTests|FullyQualifiedName~StaffArrTimekeepingAndPersonnelRecordAuthTests|FullyQualifiedName~StaffArrPersonOffboardingTests" --logger "console;verbosity=minimal"` — passed, 18 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrPersonUpdateWorkflowTests|FullyQualifiedName~StaffArrIncidentLifecycleTests|FullyQualifiedName~RoleManagementServiceTests|FullyQualifiedName~StaffArrIntegrationPermissionCheckTests" --logger "console;verbosity=minimal"` — passed, 21 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrMePortalTests|FullyQualifiedName~StaffArrPersonExportTests|FullyQualifiedName~StaffArrTenantSettingsTests|FullyQualifiedName~StaffArrWorkerAdminTests" --logger "console;verbosity=minimal"` — passed, 35 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrWorkforceOnboardingJourneyTests|FullyQualifiedName~StaffArrEventFeedTests|FullyQualifiedName~StaffArrAuditTimelineTests|FullyQualifiedName~StaffArrFieldInboxTests" --logger "console;verbosity=minimal"` — passed, 13 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrHandoffApiTests.Handoff_redeem_happy_path_returns_session_and_me_works|FullyQualifiedName~StaffArrHandoffApiTests.Session_bootstrap_returns_claim_backed_identity|FullyQualifiedName~StaffArrHandoffApiTests.People_crud_happy_path|FullyQualifiedName~StaffArrHandoffApiTests.Organization_hierarchy_crud_happy_path|FullyQualifiedName~StaffArrHandoffApiTests.Certification_definitions_and_manual_grant_happy_path|FullyQualifiedName~StaffArrHandoffApiTests.Person_readiness_summary" --logger "console;verbosity=minimal"` — passed, 3 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrPersonExportDeliveryNotificationTests|FullyQualifiedName~StaffArrPersonExportPresetTests|FullyQualifiedName~StaffArrPersonExportDeliveryWorkerTests|FullyQualifiedName~StaffArrEntityExportTests" --logger "console;verbosity=minimal"` — passed, 21 tests.

Additional verification:

- A combined StaffArr R1-focused cluster containing the same slices was attempted and timed out locally after 184 seconds without returning a result. The split runs above passed and are used as completion evidence.

Remaining blockers:

- No known StaffArr R1 foundation-spine blocker remains in the audited slice.
- `ST-WF-014` remains a documented partial workflow until the R4 training and qualification gate pass.

## R4 Training and qualification gate pass

Status: Clear for R4 after StaffArr/TrainArr acknowledgement, readiness, onboarding, incident-routing, and settings verification.

R4 scope audited:

- StaffArr has 21 R4 feature rows and 1 R4 workflow row (`ST-WF-003` onboarding and first-day readiness) in the roadmap rollout maps.
- StaffArr remains the people, location, manager, incident, and onboarding authority. TrainArr remains the training, assignment, qualification, certificate, and evidence-workflow authority.
- StaffArr reads and displays TrainArr-owned training history, qualification checks, blockers, and acknowledgements without creating local training truth.
- StaffArr incidents can route training-compliance remediation to TrainArr through server-side permissioned flows.
- StaffArr onboarding and readiness views expose training blockers and acknowledgements as actionable context without hiding ownership or making RecordArr file-storage claims.

Completed blockers and shared fixes:

- Repaired a shared TrainArr evidence settings gap discovered during the StaffArr R4 pass. The TrainArr API and settings UI now allow `completion_certificate` as a tenant evidence type so acknowledged StaffArr-to-TrainArr assignments can accept completion certificate evidence instead of returning a misleading validation failure after the worker acknowledges the assignment.
- Added a backend assertion to prevent TrainArr default evidence settings from drifting away from the R4 completion-certificate workflow.
- Kept RecordArr document/file durability out of this pass. StaffArr/TrainArr continue to own operational training readiness and evidence metadata here; RecordArr durable retained-file authority remains governed by the RecordArr roadmap stage blocker already documented in R3.

Files touched:

- `apps/trainarr-api/TrainArr.Api/Services/TrainArrTenantSettingsService.cs`
- `apps/trainarr-frontend/src/components/TenantSettingsPanel.tsx`
- `apps/trainarr-frontend/src/components/TenantSettingsPanel.test.tsx`
- `tests/STLCompliance.StaffArr.Auth.Tests/TrainArrTenantSettingsTests.cs`
- `docs/roadmap/products/staffarr.md`

Tests run:

- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrPersonBulkImportTests|FullyQualifiedName~StaffArrPersonLookupTests|FullyQualifiedName~StaffArrPersonUpdateWorkflowTests|FullyQualifiedName~StaffArrPersonOffboardingTests|FullyQualifiedName~StaffArrTimekeepingAndPersonnelRecordAuthTests|FullyQualifiedName~StaffArrMePortalTests|FullyQualifiedName~StaffArrWorkerAdminTests" --logger "console;verbosity=minimal"` - passed, 41 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrWorkforceOnboardingJourneyTests|FullyQualifiedName~StaffArrIncidentLifecycleTests|FullyQualifiedName~StaffArrFieldsetAccessTests|FullyQualifiedName~StaffArrTenantSettingsTests|FullyQualifiedName~StaffArrProcurementApprovalAuthorityTests|FullyQualifiedName~StaffArrSupplyArrSupplyDemandTests" --logger "console;verbosity=minimal"` - passed, 25 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.StaffArr.Auth.Tests.StaffArrTrainArrTrainingAcknowledgementTests.Assignment_create_publishes_acknowledgement_and_gates_evidence_until_member_acknowledges" --logger "console;verbosity=minimal"` - passed, 1 test.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrTrainArrPersonTrainingHistoryTests|FullyQualifiedName~StaffArrTrainArrQualificationCheckConsumptionTests|FullyQualifiedName~StaffArrTrainArrIncidentRoutingTests|FullyQualifiedName~StaffArrTrainArrTrainingAcknowledgementTests|FullyQualifiedName~StaffArrTrainArrTrainingBlockerTests" --logger "console;verbosity=minimal"` - passed, 21 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~TrainArrTenantSettingsTests" --logger "console;verbosity=minimal"` - passed, 9 tests.
- `npm test -- TenantSettingsPanel.test.tsx` from `apps/trainarr-frontend` - passed, 1 file / 5 tests.
- `npm test -- TrainingAcknowledgementsPanel.test.tsx WorkforceOnboardingJourneyPanel.test.tsx ReadinessPanel.test.tsx ReadinessRollupSupervisorPanel.test.tsx ReadinessReportsPanel.test.tsx StaffArrTenantSettingsPanel.test.tsx IncidentsPanel.test.tsx MyTeamPanel.test.tsx CertificationPanel.test.tsx PeopleSection.test.tsx` from `apps/staffarr-frontend` - passed, 10 files / 52 tests.

Additional verification:

- The initial StaffArr/TrainArr consumer cluster exposed the post-acknowledgement evidence upload failure before the TrainArr evidence-type fix. The exact failing test and the full cluster passed after the fix.

Remaining blockers:

- No known StaffArr R4 blocker remains in the audited slice.
- RecordArr durable evidence/file retention is still not considered production-authoritative for retained training file storage; do not present RecordArr-backed certificate/evidence archival as complete until the RecordArr blocker is resolved in its applicable stage.

R4 stage result: StaffArr is clear for the R4 product gate. Continue R4 with TrainArr; do not advance to R5 until TrainArr also clears R4 and the suite R4 summary is updated.

## R12 Expansion pass

Status: Clear for R12 with advanced workforce expansion scope explicitly deferred.

R12 scope audited:

- StaffArr has 35 R12 feature rows and no R12 workflow rows in the roadmap rollout maps.
- The audited R12 rows are retained expansion targets for workforce analytics, role mining/access review, workforce planning, pay-equity analysis, scheduling optimization, confidential case management, succession/talent review, semantic reporting definitions, external worker/agency portals, legal-hold awareness, privacy-preserving analytics, AI-assisted proposals, and portable worker evidence.
- Existing represented StaffArr surfaces remain bounded to durable people, organization/location, permissions context, self-service, account-access delegation, timeline, exports, settings, incidents, performance/benefits/compensation administration, and integration references.
- StaffArr account-access actions still delegate credential/session truth to NexArr clients. StaffArr does not become the login source of truth, and the UI states NexArr ownership for login state, security actions, and launch eligibility.
- StaffArr person timeline, exports, and self-service panels do not claim the deferred R12 analytics, external portal, AI, legal-hold, or optimization capabilities as complete.

Files touched:

- `docs/roadmap/products/staffarr.md`

Tests run:

- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrPersonAccountAccessTests|FullyQualifiedName~StaffArrMePortalTests|FullyQualifiedName~StaffArrPersonUpdateWorkflowTests|FullyQualifiedName~StaffArrEntityExportTests|FullyQualifiedName~StaffArrTenantSettingsTests|FullyQualifiedName~StaffArrPersonExportTests" --logger "console;verbosity=minimal"` - passed, 41 tests.
- `npm test -- --run src/components/PersonAccountAccessPanel.test.tsx src/components/MeSelfServicePortalPanel.test.tsx src/components/PersonTimelinePanel.test.tsx src/components/DataExportsPanel.test.tsx` from `apps/staffarr-frontend` - passed, 4 files / 16 tests.
- `npm run test:theme` from `apps/staffarr-frontend` - passed with no theme audit violations.

Remaining blockers / explicit deferrals:

- Deferred to later R12-ready slices: simple small-team mode, one-person cross-product timeline depth, quick login provisioning refinements, skills/capability graph, transparent employee update workflow expansion, cross-location readiness board, fair scheduling/shift exchange, incident-to-support flow expansion, worker-owned portable evidence, privacy-preserving workforce analytics, rehire/multi-relationship handling, guided classification questions, role mining/access review, workforce planning/scenario modeling, compensation cycle/pay-equity analysis, advanced scheduling optimization, confidential case management, succession/talent review, people analytics semantic layer, external worker/agency portal, automated HR audits, quick-create polish, RecordArr evidence references, ReportArr projections, Compliance Core applicability/gates, Field Companion execution surface, retention/privacy/legal-hold awareness, and AI-assisted proposals.
- No misleading live StaffArr R12 product surface was identified in this pass. If any deferred capability is later pulled forward, it must preserve StaffArr ownership boundaries and rely on the owning product APIs rather than duplicating source truth.

R12 product result: StaffArr is clear for the R12 suite gate. Continue R12 with Compliance Core; do not advance beyond R12 until every applicable product clears this stage.

## Source docs

- [Feature catalog](../../products/staffarr/FEATURESET.md)
- [Workflow catalog](../../products/staffarr/WORKFLOWS.md)
- [Product manifest](../../products/staffarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
