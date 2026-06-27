# R10 — ReportArr operational reporting

Turn accumulated source events, evidence, and workflow history into reports without mutating source products.

| Field | Definition |
| --- | --- |
| Entry condition | Enough products emit source refs, events, evidence refs, status history, and read-model contracts. |
| Exit condition | Reports are exportable, schedulable, provenance-aware, and can store audit-ready outputs in RecordArr. |
| Total feature rows mapped here | 33 |
| Total workflow rows mapped here | 13 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| ReportArr | 68 | 15 | Read models, dashboards, metrics, scheduled reports, exports, provenance drillbacks, and audit-ready report outputs. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| ReportArr | 33 | 13 |

## Acceptance focus

- Pass all applicable R0 gates.
- Respect source-of-truth ownership.
- Prove the vertical slice rather than only rendering screens.
- Preserve evidence, source references, audit history, and reportability hooks.
- Keep UI unified, readable, non-noisy, and truthful in degraded states.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
