# R3 — MaintainArr flagship operational slice

Prove that real operational work creates trustworthy compliance and maintenance evidence across products.

| Field | Definition |
| --- | --- |
| Entry condition | R1/R2 foundations exist enough for person/location/evidence/compliance calls. |
| Exit condition | Asset-to-work-to-return-to-service runs end-to-end with durable state, evidence, blockers, and explainable readiness. |
| Total feature rows mapped here | 59 |
| Total workflow rows mapped here | 16 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| MaintainArr | 73 | 14 | Assets, defects, inspections, preventive maintenance, work orders, readiness, downtime, and maintenance execution. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| RecordArr | 21 | 2 |
| MaintainArr | 38 | 14 |

## Acceptance focus

- Pass all applicable R0 gates.
- Respect source-of-truth ownership.
- Prove the vertical slice rather than only rendering screens.
- Preserve evidence, source references, audit history, and reportability hooks.
- Keep UI unified, readable, non-noisy, and truthful in degraded states.

## Suite-stage gate summary

Status: R3 complete for the suite with deferred RecordArr provider-grade evidence-vault blockers carried.

Completed products:

- RecordArr - product pass completed with deferred blockers. RecordArr is not clear as production-authoritative retained evidence persistence until its remaining provider-grade evidence-vault hardening closes.
- MaintainArr - clear for R3 operational maintenance execution after focused test-fixture and RecordArr-default hardening.

Not applicable products:

- NexArr, StaffArr, Compliance Core, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, RoutArr, ReportArr, Field Companion, and LedgArr have no R3 rollout rows in the roadmap authority.

Shared fixes completed in this stage:

- `OrdArrCustomArrHandoffTests` now constructs `CustomArrStore` with a durable `CustomArrDbContext`, matching CustomArr's current persistence boundary and unblocking MaintainArr backend test compilation.
- MaintainArr tenant defaults now leave `SendCompletedPacketsToRecordArr` and `EnableRecordArrDocumentPackets` disabled by default so the R3 slice does not imply automatic packet delivery into a RecordArr evidence vault that remains blocked by its remaining provider-grade hardening.

Deferred blockers carried forward:

- RecordArr's remaining provider-grade DMS/evidence-vault hardening remains unresolved. The current repo has already cleared the old singleton-store blocker for the audited current slice, but later stages must not treat RecordArr retained evidence, retained outputs, audit-governance evidence, or provider-backed share/signature/redaction execution as fully production-authoritative until that blocker is closed.
- MaintainArr local work-order, defect, and inspection evidence is operational maintenance attachment data. It is not a substitute for RecordArr-owned retained evidence.

Tests run:

- RecordArr: current repo-state reruns passed 4 auth tests, 93 focused store/integration/print-provider tests, and 12 frontend tests.
- MaintainArr: full backend `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` timed out before completion; split R3 clusters passed.
- MaintainArr split backend clusters passed across handoff, rules, asset readiness/status/downtime, import/catalog/meter/reference, PM/inspection, defect/evidence/escalation, work orders/labor/evidence, SupplyArr/StaffArr coordination, history/reports, notifications/platform events/field inbox, audit/export/reports, tenant settings, scheduling, smart import, cross-product handoff, and voice normalization.
- MaintainArr current repo-state reruns passed 10 handoff tests, 28 readiness/notification/import tests, 15 work-order-status and labor-evidence tests, 6 representative `MaintainArrWorkOrderTests`, and 4 frontend settings tests. The broader `MaintainArrWorkOrderTests|WorkOrderStatusRulesTests|MaintainArrWorkOrderLaborEvidenceTests` rerun timed out, so the narrower reruns are the reliable current evidence.
- MaintainArr explicit backend reservation/vendor/parts-kit filter found no direct tests.
- MaintainArr frontend: `npm test` from `apps/maintainarr-frontend` - passed 52 files / 145 tests.
- MaintainArr frontend settings check: `npm test -- SettingsSection.test.tsx` - passed 1 file / 4 tests after the RecordArr default repair.

Stage advancement decision:

- The suite may advance to R4.
- The RecordArr provider-grade evidence-vault blocker remains carried and must not be treated as closed by later stages.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
