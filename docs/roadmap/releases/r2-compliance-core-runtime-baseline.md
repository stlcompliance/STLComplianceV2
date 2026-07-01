# R2 — Compliance Core runtime baseline

Let products ask for applicability, required evidence, missing facts, and review outcomes without hardcoding regulatory meaning.

| Field | Definition |
| --- | --- |
| Entry condition | R1 identity, StaffArr context, RecordArr evidence references, and service-token patterns exist. |
| Exit condition | The first operational rule/evidence spine can produce unknown/conflict/missing/evidence-needed outcomes and bind to product workflows. |
| Total feature rows mapped here | 40 |
| Total workflow rows mapped here | 16 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| Compliance Core | 79 | 17 | Regulatory meaning, applicability, citations, rulepacks, evidence requirements, questionnaires, and normalized compliance facts. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| Compliance Core | 40 | 16 |

## Acceptance focus

- Pass all applicable R0 gates.
- Respect source-of-truth ownership.
- Prove the vertical slice rather than only rendering screens.
- Preserve evidence, source references, audit history, and reportability hooks.
- Keep UI unified, readable, non-noisy, and truthful in degraded states.

## Suite-stage gate summary

Status: R2 complete for the suite.

Completed products:

- Compliance Core - completed. This is the only product with R2 rollout rows.

Not applicable products:

- NexArr, StaffArr, RecordArr, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, RoutArr, ReportArr, Field Companion, and LedgArr have no R2 rollout rows in the roadmap authority.

Shared fixes completed in this stage:

- Compliance Core API startup now maps the implemented R2 report endpoint groups for report index, findings, operator status, missing evidence, evidence completeness, waivers, exceptions/exemptions, product integration health, audit readiness, remediation queue, regulatory domain coverage, HazMat table coverage, Title 49 citation coverage, citation review, rule-change impact, Title 49 coverage explorer, and evaluation history explorer.
- Title 49 citation coverage is scoped to the HMR report families (`title49_hmr`, `phmsa_hmr`, and `title49_hmr*` packs) so citation-review records with a broad `49 CFR` source prefix do not distort HMR legal-state coverage.

Deferred blockers carried forward:

- RecordArr's remaining provider-grade evidence-vault hardening remains unresolved from the prior stage. Compliance Core R2 may provide regulatory interpretation, evidence requirements, missing-evidence warnings, audit package metadata, and runtime guidance, but retained evidence files, retained outputs, and final provider-backed evidence execution remain RecordArr-owned truth and must not be represented as fully production-authoritative until RecordArr closes that blocker.

Tests run:

- `npm test -- client.test.ts ProductWorkspaceLayout.test.tsx RequirementDetailPage.test.tsx EvaluationSection.test.tsx RegistryDetailProfile.test.tsx RegistrySection.test.tsx ReportsSection.test.tsx` from `apps/compliancecore-frontend` - passed 7 files / 11 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreRuleEvaluationTests|FullyQualifiedName~ComplianceCoreInternalRuleEvaluationTests|FullyQualifiedName~ComplianceCoreFindingsWorkflowGateTests|FullyQualifiedName~ComplianceCoreMissingEvidenceWarningTests|FullyQualifiedName~ComplianceCoreProductFactMirrorTests" --logger "console;verbosity=minimal"` - passed 53 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreAuditPackageTests|FullyQualifiedName~ComplianceCoreAuditPackageGenerationTests|FullyQualifiedName~ComplianceCoreAuditDeliveryOrchestrationTests|FullyQualifiedName~AuditPackageGenerationRulesTests|FullyQualifiedName~ComplianceCoreAuditReadyFactRequirementTests" --logger "console;verbosity=minimal"` - passed 24 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreSourceIngestionTests|FullyQualifiedName~ComplianceCoreStagedImportWizardTests|FullyQualifiedName~ComplianceCoreFactSourceRegistryTests|FullyQualifiedName~ComplianceCoreFactSourceSyncWorkerTests|FullyQualifiedName~ComplianceCoreVocabularySpineTests" --logger "console;verbosity=minimal"` - passed 37 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreQuestionnaireTests|FullyQualifiedName~ComplianceCoreTheoreticalSituationTests|FullyQualifiedName~ComplianceCoreWaiverTests|FullyQualifiedName~ComplianceCoreRiskScoringTests|FullyQualifiedName~ComplianceCoreReadinessForecastTests" --logger "console;verbosity=minimal"` - passed 37 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreRegulatoryMappingsTests|FullyQualifiedName~ComplianceCoreRegulatoryRegistriesTests|FullyQualifiedName~ComplianceCoreCitationFactCatalogTests|FullyQualifiedName~ComplianceCoreSdsHazComRuleVersionTests|FullyQualifiedName~ComplianceCoreTitle49" --logger "console;verbosity=minimal"` - passed 31 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.ComplianceCore.Auth.Tests.ComplianceCoreReportTests.Title49_citation_coverage_report_summary_enumerates_legal_states" --logger "console;verbosity=minimal"` - passed 1 test after report scoping repair.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --filter "FullyQualifiedName~ComplianceCoreRuleChangeMonitoringTests|FullyQualifiedName~ComplianceCoreControlEffectivenessTests|FullyQualifiedName~ComplianceCoreOperatorDashboardTests|FullyQualifiedName~ComplianceCoreReportTests|FullyQualifiedName~ComplianceCoreScheduledEvaluationWorkerTests" --logger "console;verbosity=minimal"` - passed 39 tests.
- `dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj --logger "console;verbosity=minimal"` - timed out before completion; split R2 backend clusters passed as listed above.
- Current repo-state reruns for the same frontend and split backend clusters also passed: 11 frontend tests; backend clusters passed 53, 24, 37, 37, 31, 1, and 39 tests, respectively.

Stage advancement decision:

- The suite may advance to R3.
- The RecordArr provider-grade evidence-vault blocker remains carried and must not be treated as closed by later stages.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
