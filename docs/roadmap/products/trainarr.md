# TrainArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `trainarr` |
| Category | LMS / qualifications |
| Entry release | R4 — Training and qualification gate |
| Completion release | R4 — Training and qualification gate |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Training definitions, assignments, evaluation, certificates, qualifications, remediation, and renewals. |
| Roadmap slice | Qualification and retraining gate |
| Must not violate | Own qualification truth while StaffArr owns people and incidents. |
| Feature rows retained | 73 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R4 | Training and qualification gate | 38 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R4 unless they are only supporting another release gate.
- Common category baseline remains retained for R4.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## R0 Trust Gate pass

Status: Clear for R0 after focused launch/session hardening.

Completed blockers:

- Removed the stale TrainArr session/me `hasTrainArrAccess` success flag from API contracts, frontend types, frontend normalization, and current handoff tests.
- Stopped passing legacy NexArr launchable-product claims through TrainArr handoff and session bootstrap responses. TrainArr now returns a fixed ordinary-suite launch catalog and excludes Compliance Core from normal tenant product switching.
- Renamed the local authorization shim from entitlement wording to launch-context wording while preserving role/action checks for training definitions, assignments, evaluations, qualifications, settings, and audit/package actions.

Files touched:

- `apps/trainarr-api/TrainArr.Api/Contracts/AuthContracts.cs`
- `apps/trainarr-api/TrainArr.Api/Services/HandoffAuthService.cs`
- `apps/trainarr-api/TrainArr.Api/Services/MeService.cs`
- `apps/trainarr-api/TrainArr.Api/Services/TrainArrAuthorizationService.cs`
- `apps/trainarr-api/TrainArr.Api/Services/TrainArrSuiteLaunchCatalog.cs`
- `apps/trainarr-frontend/src/api/client.ts`
- `apps/trainarr-frontend/src/api/client.test.ts`
- `apps/trainarr-frontend/src/api/types.ts`
- `apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx`
- `tests/STLCompliance.StaffArr.Auth.Tests/TrainArrHandoffApiTests.cs`

Tests run:

- `dotnet build apps/trainarr-api/TrainArr.Api/TrainArr.Api.csproj --no-restore` - passed with existing warnings.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~TrainArrHandoffApiTests" --logger "console;verbosity=minimal"` - passed 4 tests.
- `npm test -- client.test.ts ProductWorkspaceLayout.test.tsx` from `apps/trainarr-frontend` - passed 2 files / 6 tests.

Remaining blockers: None identified in this R0 slice.

R0 stage result: TrainArr is clear to advance when the suite reaches the next stage gate.

## R1 Foundation spine pass

Status: Not applicable. TrainArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R4.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no TrainArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no TrainArr rows for `R1`.
- TrainArr's product FEATURESET and WORKFLOWS remain retained full scope, but they do not authorize starting the R4 training and qualification gate during the R1 suite stage.

Files touched:

- `docs/roadmap/products/trainarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no TrainArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. TrainArr must wait for the suite to reach R4 before qualification-gate work begins.

R1 stage result: TrainArr is clear for the R1 suite gate as not applicable.

## R4 Training and qualification gate pass

Status: Clear for R4 with explicit deferrals for target/common baseline workflows that are not yet implemented as full owner APIs.

R4 scope audited:

- TrainArr has 38 R4 feature rows and 13 R4 workflow rows in the roadmap rollout maps.
- Durable/current R4 workflows audited and verified: definition/version authoring, program composition, assignments and learner progress, completion rules and branching, evidence capture, evaluations/signoffs, qualification checks and issues, certificate publication, incident remediation, training matrices/applicability, recertification, rulepack impact, reminders/escalations, evidence retention/orphan checks, reports, audit packages, integrations, and StaffArr-facing readiness publication.
- TrainArr remains the training, qualification, certificate, assignment, evaluation, remediation, and training-evidence metadata authority. StaffArr remains the person, manager, location, onboarding, and personnel-incident authority. Compliance Core remains regulatory meaning. RecordArr remains file-binary and controlled-document authority.

Completed blockers and shared fixes:

- Repaired TrainArr tenant evidence settings so R4 learner-completion and StaffArr acknowledgement workflows can accept training-domain evidence types rather than failing after otherwise successful acknowledgement or self-upload.
- Aligned the server allow-list, default tenant settings, and TrainArr settings UI for domain evidence types: `completion_certificate`, `evaluation_sheet`, `signoff_form`, `practical_demo`, `attendance_roster`, and `quiz_result`, while preserving existing generic file/media evidence types.
- Added backend assertions for default training-domain evidence types so the R4 completion/evidence path cannot drift back to a misleading validation failure.

Files touched:

- `apps/trainarr-api/TrainArr.Api/Services/TrainArrTenantSettingsService.cs`
- `apps/trainarr-frontend/src/components/TenantSettingsPanel.tsx`
- `apps/trainarr-frontend/src/components/TenantSettingsPanel.test.tsx`
- `tests/STLCompliance.StaffArr.Auth.Tests/TrainArrTenantSettingsTests.cs`
- `docs/roadmap/products/trainarr.md`

Tests run:

- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.StaffArr.Auth.Tests.StaffArrTrainArrProgramEvidenceTests.Training_evidence_upload_allows_member_self" --logger "console;verbosity=minimal"` - passed, 1 test.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrTrainArrTrainingAssignmentTests|FullyQualifiedName~StaffArrTrainArrCompletionRuleTests|FullyQualifiedName~StaffArrTrainArrStepBranchTests|FullyQualifiedName~StaffArrTrainArrSignoffsEvaluationsTests|FullyQualifiedName~StaffArrTrainArrProgramEvidenceTests|FullyQualifiedName~StaffArrTrainArrProgramContentReferenceTests|FullyQualifiedName~StaffArrTrainArrCitationAttachmentTests" --logger "console;verbosity=minimal"` - passed, 35 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrTrainArrPersonTrainingHistoryTests|FullyQualifiedName~StaffArrTrainArrQualificationCheckConsumptionTests|FullyQualifiedName~StaffArrTrainArrIncidentRoutingTests|FullyQualifiedName~StaffArrTrainArrTrainingAcknowledgementTests|FullyQualifiedName~StaffArrTrainArrTrainingBlockerTests" --logger "console;verbosity=minimal"` - passed, 21 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrTrainArrQualificationCheckTests|FullyQualifiedName~StaffArrTrainArrQualificationBatchCheckTests|FullyQualifiedName~StaffArrTrainArrQualificationGrantTests|FullyQualifiedName~StaffArrTrainArrQualificationLifecycleTests" --logger "console;verbosity=minimal"` - passed, 35 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrTrainArrQualificationExpirationWorkerTests|FullyQualifiedName~StaffArrTrainArrRecertificationAssignmentWorkerTests|FullyQualifiedName~StaffArrTrainArrQualificationRecalculationWorkerTests|FullyQualifiedName~StaffArrTrainArrPublicationRetryWorkerTests" --logger "console;verbosity=minimal"` - passed, 13 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~StaffArrTrainArrRulePackRequirementTests|FullyQualifiedName~StaffArrTrainArrRulePackImpactTests|FullyQualifiedName~StaffArrTrainArrRulePackImpactWorkerTests|FullyQualifiedName~TrainArrIntegrationSettingsTests|FullyQualifiedName~StaffArrTrainArrEventProcessingWorkerTests" --logger "console;verbosity=minimal"` - passed, 27 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~TrainingNotificationRulesTests|FullyQualifiedName~TrainArrTrainingNotificationTests|FullyQualifiedName~TrainArrAssignmentReminderEscalationWorkerTests|FullyQualifiedName~AssignmentEscalationRulesTests|FullyQualifiedName~AssignmentDueReminderRulesTests|FullyQualifiedName~StaffArrTrainArrEvidenceRetentionWorkerTests|FullyQualifiedName~StaffArrTrainArrOrphanReferenceWorkerTests|FullyQualifiedName~TrainArrFieldInboxTests|FullyQualifiedName~TrainArrSupplyArrMaterialDemandTests" --logger "console;verbosity=minimal"` - passed, 35 tests.
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~TrainArrReportTests|FullyQualifiedName~TrainArrAuditPackageTests|FullyQualifiedName~TrainArrAuditPackageGenerationTests|FullyQualifiedName~TrainArrAuditPackageGenerationRulesTests|FullyQualifiedName~TrainArrTenantSettingsTests|FullyQualifiedName~TrainArrHandoffApiTests|FullyQualifiedName~TrainArrLoadTestJourneySeedTests" --logger "console;verbosity=minimal"` - passed, 53 tests.
- `npm test` from `apps/trainarr-frontend` - passed, 50 files / 110 tests.

Additional verification:

- A combined qualification/certification backend cluster exceeded the local 5-minute command timeout and left a stale testhost. The stale process was stopped and the same coverage was rerun in smaller passing clusters.
- The initial authoring/evidence cluster exposed the member self-upload validation failure before the evidence-type fix. The exact failing test and the full cluster passed after the fix.

Deferred blockers:

- `TR-WF-007` instructor-led session scheduling and attendance remains a target/common baseline workflow. The current app has instructor console surfaces and assignment/manual completion support, but not a complete session/capacity/waitlist/calendar/attendance owner API. Defer full session scheduling implementation to the retained TrainArr category-depth backlog.
- `TR-WF-011` external credential review and equivalency remains a target/common underserved workflow. Current qualification and evidence flows can record training outcomes, but not a full external issuer verification, equivalency mapping, appeal, and re-verification workflow. Defer full equivalency implementation to the retained TrainArr category-depth backlog.
- Some R4 target feature rows that describe broader LMS category depth, including standards interoperability, surveys/feedback, saved views, bulk operations, mapping import, and professional report layout breadth, remain retained scope rather than newly expanded in this pass. They were not removed or represented as complete.
- RecordArr durable retained-file storage remains outside this TrainArr pass. TrainArr evidence metadata and local evidence storage were verified, but retained file-binary authority must wait for the RecordArr durable evidence blocker to be resolved.

R4 stage result: TrainArr is clear for the R4 product gate with the deferred blockers above. The R4 suite gate may close after the R4 release summary records StaffArr and TrainArr completion plus these deferrals.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for R12 with explicit deferrals for full offline learning, skills development planning, AI generation, external academies, and category-depth workflows that are retained but not yet complete.

R12 scope audited:

- TrainArr has 35 R12 feature rows and 2 R12 workflow rows in the roadmap rollout maps.
- R12-adjacent durable/current slices audited: qualification wallet verification, catalog-assisted program draft suggestions, qualification/readiness reports, training gap and citation gap reports, Field Companion inbox, rulepack impact handling, evidence retention, orphan-reference checks, tenant settings, StaffArr acknowledgement, qualification lifecycle, and training matrix/qualification checks.
- TrainArr remains the learning, qualification, certificate, remediation, and training-evidence metadata authority. StaffArr remains person/workforce truth, Compliance Core remains regulatory meaning, RecordArr remains file-binary and controlled-document authority, Field Companion remains the mobile/offline execution surface, and ReportArr remains reporting/read-model authority.

Completed blockers:

- Replaced misleading TrainArr "AI-assisted draft" copy with truthful "catalog-assisted draft" wording in the deterministic training-program draft generator, frontend builder panel, and tests. The existing route remains permissioned and useful, but no longer represents keyword matching as AI generation.
- Verified no remaining TrainArr API/frontend/test user-facing `AI-assisted` wording in the current slice.

Files touched:

- `apps/trainarr-api/TrainArr.Api/Services/TrainingProgramDraftService.cs`
- `apps/trainarr-frontend/src/components/ProgramBuilderPanel.tsx`
- `apps/trainarr-frontend/src/components/ProgramBuilderPanel.test.tsx`
- `tests/STLCompliance.StaffArr.Auth.Tests/TrainArrReportTests.cs`
- `docs/roadmap/products/trainarr.md`

Tests run:

- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~TrainArrReportTests|FullyQualifiedName~TrainArrTenantSettingsTests|FullyQualifiedName~TrainArrHandoffApiTests|FullyQualifiedName~TrainArrFieldInboxTests|FullyQualifiedName~StaffArrTrainArrQualificationLifecycleTests|FullyQualifiedName~StaffArrTrainArrQualificationCheckTests|FullyQualifiedName~StaffArrTrainArrTrainingAcknowledgementTests" --logger "console;verbosity=minimal"` - passed 61 tests.
- `npm test -- --run src/components/ProgramBuilderPanel.test.tsx src/components/QualificationWalletPanel.test.tsx src/components/QualificationReportsPanel.test.tsx src/components/TrainingMatrixPanel.test.tsx src/components/EvidenceRetentionSettingsPanel.test.tsx src/components/OrphanReferenceSettingsPanel.test.tsx src/workspace/sections/ReportsSection.test.tsx` from `apps/trainarr-frontend` - passed 7 files / 14 tests.
- `npm run test:theme` from `apps/trainarr-frontend` - passed with no theme audit violations.

Deferred blockers:

- `TR-WF-012` offline mobile learning, checklist, and sync remains retained R12 scope. Current Field Companion inbox and TrainArr APIs are useful online/owned surfaces, but TrainArr does not yet have the full offline download, encrypted cache, queued action, conflict-resolution, and server revalidation workflow described by the workflow contract.
- `TR-WF-015` skills gap and development plan remains retained R12 scope. Current qualification checks, matrices, reports, and readiness alerts can expose gaps, but not the full governed skill ontology, employee/manager development plan, coaching assignment, progression tracking, and reassessment lifecycle.
- Full R12 AI capabilities (`TR-DEM-001`, `TR-DEM-003`, `TR-DEM-004`, `TR-DEM-010`, `TR-FND-020`) remain deferred. The current catalog-assisted draft is deterministic and reviewable; it must not be treated as AI course generation, adaptive learning, simulation authoring, multilingual transformation, or consequential AI proposal automation.
- External/customer/partner academy and external credential equivalency depth (`TR-DEM-006`, `TR-UND-007`) remains deferred beyond existing controlled settings, evidence, qualification, and wallet verification paths. No external self-registration academy, issuer verification, equivalency mapping, appeal, or re-verification workflow was introduced.
- RecordArr durable retained-file storage remains an external blocker for TrainArr evidence packages that require final file-binary authority.

R12 stage result: TrainArr is clear for the R12 product gate with the deferred blockers above. The suite must continue R12 with SupplyArr next; TrainArr must not advance to later-stage work until every applicable product completes R12.

## Source docs

- [Feature catalog](../../products/trainarr/FEATURESET.md)
- [Workflow catalog](../../products/trainarr/WORKFLOWS.md)
- [Product manifest](../../products/trainarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
