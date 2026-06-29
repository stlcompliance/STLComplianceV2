# Compliance Core Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `compliancecore` |
| Category | GRC / rule engine |
| Entry release | R2 — Compliance Core runtime baseline |
| Completion release | R2 — Compliance Core runtime baseline |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Regulatory meaning, applicability, citations, rulepacks, evidence requirements, questionnaires, and normalized compliance facts. |
| Roadmap slice | Compliance guidance baseline |
| Must not violate | Keep administrative authoring platform-admin-only while runtime guidance serves all products. |
| Feature rows retained | 79 |
| Workflow rows retained | 17 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R2 | Compliance Core runtime baseline | 40 | 16 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 39 | 1 |

## Implementation interpretation

- Current/represented capabilities are hardened in R2 unless they are only supporting another release gate.
- Common category baseline remains retained for R2.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## R0 Trust Gate pass

Status: Clear for R0 after focused admin-session hardening.

Completed blockers:

- Removed the stale Compliance Core session/me `hasComplianceCoreAccess` success flag from API contracts, frontend types, and current tests so the admin shell no longer presents a product access boolean as domain truth.
- Replaced Compliance Core handoff/session launchable-product passthrough with a fixed platform-admin suite catalog. The Compliance Core admin UI remains platform-admin-only, while runtime authorization continues to use Compliance Core runtime context and role/action checks for tenant workflows.
- Renamed the current authorization helper from entitlement wording to runtime-context wording without changing runtime guidance semantics.
- Preserved platform-admin-only session and handoff checks for Compliance Core administrative UI, including tests that forbid non-platform-admin users.

Files touched:

- `apps/compliancecore-api/ComplianceCore.Api/Contracts/AuthContracts.cs`
- `apps/compliancecore-api/ComplianceCore.Api/Services/ComplianceCoreAuthorizationService.cs`
- `apps/compliancecore-api/ComplianceCore.Api/Services/ComplianceCoreSuiteLaunchCatalog.cs`
- `apps/compliancecore-api/ComplianceCore.Api/Services/HandoffAuthService.cs`
- `apps/compliancecore-api/ComplianceCore.Api/Services/MeService.cs`
- `apps/compliancecore-frontend/src/api/types.ts`
- `apps/compliancecore-frontend/src/api/client.test.ts`
- `apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.test.tsx`
- `apps/compliancecore-frontend/src/pages/requirements/RequirementDetailPage.test.tsx`
- `apps/compliancecore-frontend/src/workspace/sections/EvaluationSection.test.tsx`
- `apps/compliancecore-frontend/src/workspace/sections/RegistryDetailProfile.test.tsx`
- `apps/compliancecore-frontend/src/workspace/sections/RegistrySection.test.tsx`
- `apps/compliancecore-frontend/src/workspace/sections/ReportsSection.test.tsx`
- `tests/STLCompliance.ComplianceCore.Auth.Tests/ComplianceCoreHandoffApiTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreHandoffApiTests.Handoff_redeem_happy_path_returns_session_and_me_works|FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreHandoffApiTests.V1_handoff_session_and_me_aliases_work|FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreHandoffApiTests.Session_bootstrap_returns_claim_backed_identity_after_non_compliancecore_launch_context|FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreHandoffApiTests.Session_and_me_forbid_non_platform_admin_users_even_with_compliancecore_launch_context|FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreHandoffApiTests.Handoff_redeem_forbids_non_platform_admin_users|FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreHandoffApiTests.Me_forbids_users_without_platform_admin_access" --logger "console;verbosity=minimal"` - passed 6 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~ComplianceCoreHandoffApiTests" --logger "console;verbosity=minimal"` - passed, 8 tests in 1m 7s.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~ComplianceCoreHandoffApiTests|FullyQualifiedName~ComplianceCoreAuthRateLimitingTests|FullyQualifiedName~ComplianceCoreAuditReadyFactRequirementTests|FullyQualifiedName~ComplianceCoreInternalRuleEvaluationTests" --logger "console;verbosity=minimal"` - passed, 15 tests in 1m 17s.
- `npm test -- client.test.ts ProductWorkspaceLayout.test.tsx RequirementDetailPage.test.tsx EvaluationSection.test.tsx RegistryDetailProfile.test.tsx RegistrySection.test.tsx ReportsSection.test.tsx` from `apps/compliancecore-frontend` - passed 7 files / 11 tests.

Remaining blockers: None identified in this R0 slice. Older internal Compliance Core test helpers outside the handoff/session slice still use `entitlements` as a token-helper parameter name; they are not user-facing, and a broad mechanical rename is deferred until those tests are touched by their owning stage or a suite-wide terminology cleanup.

R0 stage result: Compliance Core is clear to advance when the suite reaches the next stage gate.

## R1 Foundation spine pass

Status: Not applicable.

Justification:

- The rollout authority assigns Compliance Core entry and completion to `R2 — Compliance Core runtime baseline`; the R1 feature/workflow rollout maps contain no Compliance Core rows.
- Compliance Core R0 administrative-session hardening remains in force: administrative authoring UI stays platform-admin-only, while runtime/guidance semantics are reserved for the R2 pass.
- No R1 code or UI expansion was introduced.

Tests run:

- Not run for R1; no Compliance Core R1 implementation slice is applicable.

Remaining blockers:

- None for R1. Compliance Core begins at R2 under the stage-gated roadmap.

## R2 Compliance Core runtime baseline pass

Status: Clear for R2 after focused runtime report-route and Title 49 coverage hardening.

Scope audited:

- The R2 rollout authority maps 40 Compliance Core feature rows and 16 Compliance Core workflow rows to this stage.
- The pass stayed inside the R2 runtime baseline: regulatory meaning, applicability, citations, rulepacks, fact/evidence requirements, product fact mirrors, workflow gates, findings/remediation, waivers, regulatory change impact, readiness/risk analytics, questionnaires, SDS/HazCom, audit/export packages, administrative workspace, common regulatory library, and runtime/reportability hooks.
- No R1/R3/R12 expansion was introduced.

Completed blockers:

- Mapped the existing Compliance Core report endpoint groups in the API startup path so R2 runtime/reportability surfaces are reachable instead of returning truthful-but-blocking 404s for implemented report services.
- Scoped the Title 49 citation coverage report to the HMR report families (`title49_hmr`, `phmsa_hmr`, and `title49_hmr*` packs) so independent citation-review rows sharing a `49 CFR` source prefix do not pollute legal-state coverage counts.
- Preserved Compliance Core ownership boundaries: Compliance Core interprets regulatory meaning and reportability; RecordArr remains the owner of retained files, document metadata, versioning, retention, access history, and controlled document lifecycle.

Cross-product note:

- RecordArr's carried durable evidence-store blocker remains external to this Compliance Core R2 pass. Compliance Core can reference evidence requirements, missing-evidence warnings, audit package metadata, and runtime guidance, but the suite must not represent RecordArr as production-authoritative retained evidence persistence until that RecordArr blocker closes.

Files touched:

- `apps/compliancecore-api/ComplianceCore.Api/Program.cs`
- `apps/compliancecore-api/ComplianceCore.Api/Services/Title49CitationCoverageReportService.cs`
- `docs/roadmap/products/compliancecore.md`
- `docs/roadmap/releases/r2-compliance-core-runtime-baseline.md`

Tests run:

- `npm test -- client.test.ts ProductWorkspaceLayout.test.tsx RequirementDetailPage.test.tsx EvaluationSection.test.tsx RegistryDetailProfile.test.tsx RegistrySection.test.tsx ReportsSection.test.tsx` from `apps/compliancecore-frontend` - passed 7 files / 11 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreRuleEvaluationTests|FullyQualifiedName~ComplianceCoreInternalRuleEvaluationTests|FullyQualifiedName~ComplianceCoreFindingsWorkflowGateTests|FullyQualifiedName~ComplianceCoreMissingEvidenceWarningTests|FullyQualifiedName~ComplianceCoreProductFactMirrorTests" --logger "console;verbosity=minimal"` - passed 53 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreAuditPackageTests|FullyQualifiedName~ComplianceCoreAuditPackageGenerationTests|FullyQualifiedName~ComplianceCoreAuditDeliveryOrchestrationTests|FullyQualifiedName~AuditPackageGenerationRulesTests|FullyQualifiedName~ComplianceCoreAuditReadyFactRequirementTests" --logger "console;verbosity=minimal"` - passed 24 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreSourceIngestionTests|FullyQualifiedName~ComplianceCoreStagedImportWizardTests|FullyQualifiedName~ComplianceCoreFactSourceRegistryTests|FullyQualifiedName~ComplianceCoreFactSourceSyncWorkerTests|FullyQualifiedName~ComplianceCoreVocabularySpineTests" --logger "console;verbosity=minimal"` - passed 37 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreQuestionnaireTests|FullyQualifiedName~ComplianceCoreTheoreticalSituationTests|FullyQualifiedName~ComplianceCoreWaiverTests|FullyQualifiedName~ComplianceCoreRiskScoringTests|FullyQualifiedName~ComplianceCoreReadinessForecastTests" --logger "console;verbosity=minimal"` - passed 37 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreRegulatoryMappingsTests|FullyQualifiedName~ComplianceCoreRegulatoryRegistriesTests|FullyQualifiedName~ComplianceCoreCitationFactCatalogTests|FullyQualifiedName~ComplianceCoreSdsHazComRuleVersionTests|FullyQualifiedName~ComplianceCoreTitle49" --logger "console;verbosity=minimal"` - passed 31 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreReportTests.Title49_citation_coverage_report_summary_enumerates_legal_states" --logger "console;verbosity=minimal"` - passed 1 test after report scoping repair.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreRuleChangeMonitoringTests|FullyQualifiedName~ComplianceCoreControlEffectivenessTests|FullyQualifiedName~ComplianceCoreOperatorDashboardTests|FullyQualifiedName~ComplianceCoreReportTests|FullyQualifiedName~ComplianceCoreScheduledEvaluationWorkerTests" --logger "console;verbosity=minimal"` - passed 39 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --logger "console;verbosity=minimal"` - timed out before completion; the R2 backend surface was verified through the split test clusters above.

Remaining blockers:

- None identified inside the Compliance Core R2 runtime baseline.
- External carried blocker: RecordArr durable retained-evidence persistence remains unresolved and must continue to be called out when workflows depend on production-authoritative stored evidence.

R2 stage result: Compliance Core is clear for R2. Because Compliance Core is the only product with R2 rollout rows, the suite may advance to R3 after the R2 suite summary is recorded, with the RecordArr evidence-store dependency carried forward.

## R12 Expansion pass

Status: Clear for R12 after Compliance Core analytics-copy hardening, with advanced expansion scope explicitly deferred.

R12 scope audited:

- Compliance Core has 39 R12 feature rows and 1 R12 workflow row (`CC-WF-017`, Compliance Core service degradation) in the roadmap rollout maps.
- The audited R12 rows are retained expansion targets for plain-language applicability, requirement-to-workflow traceability, evidence reuse, operational compliance in the flow of work, conflict-aware facts, auditor/assurance exchange, scenario testing, continuous monitoring, cross-framework harmonization, cited AI assistance, policy-as-code, quantitative risk, control-effectiveness analytics, automated evidence connectors, knowledge/impact graphs, readiness forecasting, audit sampling, digital twin, control-owner certification, and shared foundation behaviors.
- Existing represented advanced analytics surfaces remain durable administrative/runtime slices for risk scoring, missing-evidence warnings, control effectiveness, readiness forecasting, theoretical situations, audit package delivery, and scheduled worker orchestration. They do not silently commit operational product records.
- Compliance Core administrative authoring and worker settings remain platform-admin/compliance-admin surfaces; runtime/guidance remains available to tenant workflows through server-side checks.

Completed blockers and copy fixes:

- Removed user-facing `M12`, `W47`, and `W231` worker shorthand from Compliance Core analytics/audit orchestration copy so admin UI describes the real capability instead of exposing roadmap/internal identifiers.
- Reworded visible frontend API fallback messages and backend settings/validation messages from `M12 analytics` to `compliance analytics` while preserving existing API routes, contracts, setting keys, worker class names, and test IDs for compatibility.
- Left internal endpoint names, type names, and service-worker authorization identifiers unchanged because they are implementation/API surface rather than normal user copy.

Files touched:

- `apps/compliancecore-api/ComplianceCore.Api/Endpoints/SettingsEndpoints.cs`
- `apps/compliancecore-api/ComplianceCore.Api/Services/M12AnalyticsBatchWorkerService.cs`
- `apps/compliancecore-frontend/src/api/client.ts`
- `apps/compliancecore-frontend/src/components/AuditDeliveryOrchestrationPanel.tsx`
- `apps/compliancecore-frontend/src/components/M12AnalyticsWorkerSettingsPanel.tsx`
- `docs/roadmap/products/compliancecore.md`

Tests run:

- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreM12AnalyticsBatchWorkerTests|FullyQualifiedName~ComplianceCoreAuditDeliveryOrchestrationTests|FullyQualifiedName~ComplianceCoreFactSourceSyncWorkerTests|FullyQualifiedName~ComplianceCoreControlEffectivenessTests|FullyQualifiedName~ComplianceCoreReadinessForecastTests|FullyQualifiedName~ComplianceCoreTheoreticalSituationTests" --logger "console;verbosity=minimal"` - passed, 31 tests.
- `npm test -- --run src/components/AuditDeliveryOrchestrationPanel.test.tsx src/components/M12AnalyticsWorkerSettingsPanel.test.tsx src/components/ControlEffectivenessPanel.test.tsx src/components/ReadinessForecastPanel.test.tsx src/pages/theoretical-situation/TheoreticalSituationPage.test.tsx` from `apps/compliancecore-frontend` - passed, 4 files / 9 tests.
- `npm run test:theme` from `apps/compliancecore-frontend` - passed with no theme audit violations.

Remaining blockers / explicit deferrals:

- `CC-WF-017` service-degradation workflow remains a retained target. Existing component-level degraded/loading/error states are truthful, but the full cross-product degradation state machine, fail-open/fail-closed policy registry, replay/reconciliation flow, and postmortem workflow are not complete in this pass.
- Deferred to later R12-ready slices: customer/supplier assurance exchange, external auditor room/portal, cited AI extraction assistance, continuous-control monitoring breadth, cross-framework harmonization depth, automated evidence connectors, knowledge/impact graph navigation, advanced audit sampling, federated assurance claims, digital twin expansion, and control-owner certification.
- RecordArr durable retained-evidence persistence remains the carried external dependency for workflows that require production-authoritative stored evidence, packages, or legal-hold-aware source files.

R12 product result: Compliance Core is clear for the R12 suite gate. Continue R12 with RecordArr; do not advance beyond R12 until every applicable product clears this stage.

## Source docs

- [Feature catalog](../../products/compliancecore/FEATURESET.md)
- [Workflow catalog](../../products/compliancecore/WORKFLOWS.md)
- [Product manifest](../../products/compliancecore/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
