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

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
