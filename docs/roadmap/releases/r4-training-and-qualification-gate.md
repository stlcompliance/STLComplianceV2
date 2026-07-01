# R4 — Training and qualification gate

Make person readiness and qualification checks real so operational work can be gated by training truth.

| Field | Definition |
| --- | --- |
| Entry condition | StaffArr people/incidents and MaintainArr work contexts can reference training requirements. |
| Exit condition | Products can check qualification truth; incidents can trigger retraining; renewed qualifications update readiness without local copies. |
| Total feature rows mapped here | 59 |
| Total workflow rows mapped here | 14 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| TrainArr | 73 | 15 | Training definitions, assignments, evaluation, certificates, qualifications, remediation, and renewals. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| StaffArr | 21 | 1 |
| TrainArr | 38 | 13 |

## Acceptance focus

- Pass all applicable R0 gates.
- Respect source-of-truth ownership.
- Prove the vertical slice rather than only rendering screens.
- Preserve evidence, source references, audit history, and reportability hooks.
- Keep UI unified, readable, non-noisy, and truthful in degraded states.

## Suite-stage R4 result

Status: R4 is complete for the suite with explicit TrainArr target-workflow deferrals. The suite may advance to R5 under the stage-gated rollout rule.

Completed products:

- StaffArr: clear for R4. StaffArr training acknowledgements, onboarding readiness, incident routing, readiness rollups, self-service/team views, settings, and TrainArr consumption were audited and tested. StaffArr remains people/location/incident/onboarding authority and does not copy TrainArr training truth.
- TrainArr: clear for R4 with documented deferrals. Durable qualification-gate capabilities were audited and tested across definitions, programs, assignments, branching, learner progress, evidence, signoff/evaluation, qualifications, certificates, remediation, matrix/applicability, recertification, rulepack impact, reminders/escalations, retention/orphan checks, reports, audit packages, integrations, and StaffArr publication.

Not-applicable products:

- NexArr, Compliance Core, RecordArr, MaintainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, RoutArr, ReportArr, Field Companion, and LedgArr have no R4 inventory rows in the roadmap rollout maps. Their retained product docs remain full scope, but they were not advanced during R4.

Shared fixes:

- TrainArr tenant evidence settings now include training-domain evidence types used by R4 workflows: `completion_certificate`, `evaluation_sheet`, `signoff_form`, `practical_demo`, `attendance_roster`, and `quiz_result`.
- The TrainArr settings UI and tests were aligned with the server allow-list so acknowledged assignments and learner self-uploads do not fail with misleading evidence-type validation errors.

Deferred blockers:

- TrainArr `TR-WF-007` instructor-led session scheduling and attendance remains retained target scope. Current instructor/manual assignment surfaces are not a complete session, capacity, waitlist, calendar, and attendance owner workflow.
- TrainArr `TR-WF-011` external credential review and equivalency remains retained target scope. Current evidence and qualification flows do not yet provide full issuer verification, equivalency mapping, appeal, and re-verification.
- Broader TrainArr target/common category rows such as standards interoperability, surveys/feedback, saved views, bulk operations, import mapping, and professional report layout breadth remain retained scope and were not expanded during this pass.
- RecordArr retained-file authority is still not fully production-authoritative for training certificate/evidence binaries. R4 verifies TrainArr evidence metadata/workflow behavior, not final RecordArr-backed file retention while the remaining provider-grade evidence-vault blocker is still open.

Tests run:

- StaffArr backend R4 clusters: 41, 25, 1, 21, and 9 tests passed in the original product pass, and current repo-state reruns passed 24 TrainArr-consumer tests plus 9 TrainArr tenant-settings tests and the acknowledgement regression. A broader mixed current rerun timed out, so the narrower reruns remain the reliable current completion evidence.
- StaffArr frontend R4 slice: `npm test -- TrainingAcknowledgementsPanel.test.tsx WorkforceOnboardingJourneyPanel.test.tsx ReadinessPanel.test.tsx ReadinessRollupSupervisorPanel.test.tsx ReadinessReportsPanel.test.tsx StaffArrTenantSettingsPanel.test.tsx IncidentsPanel.test.tsx MyTeamPanel.test.tsx CertificationPanel.test.tsx PeopleSection.test.tsx` passed, 10 files / 52 tests.
- TrainArr backend R4 clusters: exact member self-upload regression passed, then 35, 21, 35, 13, 27, 35, and 53 tests passed across authoring/evidence, StaffArr consumption, qualifications/certificates, workers, rulepacks/integrations, notifications/retention/field inbox/material demand, reports/audit/settings/handoff/load-test seed. Current repo-state reruns also passed a 28-test TrainArr-consumer/handoff cluster.
- TrainArr frontend: `npm test` passed, 50 files / 110 tests. Current repo-state reruns also passed the TrainArr frontend client/layout slice with 6 tests.

Stage decision:

- R4 may advance to R5 because every applicable R4 product has completed its pass and the remaining target/common category gaps are explicitly documented as deferred retained scope rather than silently treated as finished.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
