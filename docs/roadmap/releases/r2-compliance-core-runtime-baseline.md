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

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
