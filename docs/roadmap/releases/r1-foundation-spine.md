# R1 — Foundation spine

Give every product a trustworthy way to identify users, tenants, people, locations, records, evidence, and shared reference data without shadow ownership.

| Field | Definition |
| --- | --- |
| Entry condition | R0 gates passing or explicitly tracked as release blockers. |
| Exit condition | Products can launch, authorize, reference people/locations/evidence/reference data, and expose shared page archetypes without local owner duplication. |
| Total feature rows mapped here | 65 |
| Total workflow rows mapped here | 37 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| NexArr | 69 | 14 | Identity, tenant membership, launch/session/service identity, platform admin, and account authority. |
| StaffArr | 72 | 15 | People, roles, permissions context, org structure, sites, locations, incidents, and delegated account workflows. |
| RecordArr | 69 | 15 | Document metadata, files, versions, record packets, retention, evidence references, and audit packages. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| NexArr | 36 | 11 |
| StaffArr | 16 | 14 |
| RecordArr | 13 | 12 |

## Acceptance focus

- Pass all applicable R0 gates.
- Respect source-of-truth ownership.
- Prove the vertical slice rather than only rendering screens.
- Preserve evidence, source references, audit history, and reportability hooks.
- Keep UI unified, readable, non-noisy, and truthful in degraded states.

## Suite R1 stage-gate summary

Status: Suite R1 pass completed product-by-product in rollout order.

Completed products:

- NexArr: R1 pass complete with no remaining blockers in the audited identity, launch, navigation, service-token, tenant-integration, Smart Import, platform lifecycle, and reference-data slice.
- StaffArr: R1 pass complete with no remaining blockers in the audited people, roles, org/location, self-service, export, worker/admin, incident, onboarding, handoff, and field-inbox slice.
- RecordArr: R1 pass complete with deferred blockers. The old singleton-store R0 blocker is no longer active in the audited current slice, but RecordArr still cannot be treated as the suite's fully production-authoritative evidence vault until the remaining provider-grade DMS hardening closes around immutable audit/notarization, audit-anchor/governance evidence, object-storage control-plane operations, lifecycle verification, backup/restore orchestration, and managed trust-service/redaction-provider execution.

Not applicable for R1:

- Compliance Core: no R1 rows; entry release remains R2.
- MaintainArr: no R1 rows; entry release remains R3.
- TrainArr: no R1 rows; entry release remains R4.
- SupplyArr: no R1 rows; entry release remains R5.
- LoadArr: no R1 rows; entry release remains R5.
- AssurArr: no R1 rows; entry release remains R6.
- CustomArr: no R1 rows; entry release remains R7A.
- OrdArr: no R1 rows; entry release remains R7B.
- RoutArr: no R1 rows; entry release remains R8.
- ReportArr: no R1 rows; entry release remains R10.
- Field Companion: no R1 rows; entry release remains R9.
- LedgArr: no R1 rows; entry release remains R11.

Deferred blockers carried forward:

- RecordArr's remaining provider-grade evidence-vault hardening blockers remain active and must be resolved before any downstream product relies on RecordArr as fully production-authoritative evidence persistence, retained-output authority, or final external share/signature/redaction execution truth.
- R0 deferred blockers from non-R1 products remain tracked in their product roadmap files and are not cleared by this R1 pass.

Shared fixes completed in R1:

- NexArr navigation now uses the fixed ordinary-suite catalog rather than stale launch claims for ordinary product availability.
- NexArr reference-data import rejects empty record sets instead of creating placeholder staging work.
- NexArr platform lifecycle copy no longer uses grant/revoke language for ordinary product compatibility changes.

Tests run for R1:

- NexArr focused auth/navigation/platform/reference-data/lifecycle/service-token/tenant-integration/Smart Import tests: current repo-state reruns passed 33, 42, and 74 tests across the documented split runs.
- StaffArr focused R1 test clusters: passed 18, 21, 35, 13, 3, and 21 tests across the documented split runs. One larger combined run timed out and was replaced by these split completion runs.
- RecordArr focused backend and frontend checks: current repo-state reruns passed 4 auth tests, 93 store/integration/print-provider tests, and 12 frontend tests.
- R1 not-applicable products had documentation-only passes; no code, UI, API, data-flow, or test files changed for those product-stage notes.

Stage advancement decision:

- The suite may advance to R2 with RecordArr's remaining provider-grade evidence-vault blockers explicitly carried forward. R2 work must not treat RecordArr as fully production-authoritative evidence persistence until those blockers are closed or explicitly handled inside the current R2 product-stage slice.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
