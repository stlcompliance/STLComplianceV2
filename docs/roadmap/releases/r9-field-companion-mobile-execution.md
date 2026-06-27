# R9 — Field Companion mobile execution

Put selected product actions in the field once owning APIs can enforce workflow, permissions, and idempotency.

| Field | Definition |
| --- | --- |
| Entry condition | Each mobile action has an owning product API, retry semantics, evidence rules, and clear blocked/degraded states. |
| Exit condition | Mobile/offline users can complete assigned work without Field Companion becoming a hidden source of truth. |
| Total feature rows mapped here | 33 |
| Total workflow rows mapped here | 13 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| Field Companion | 71 | 17 | Mobile assigned work, secure capture/upload, offline queueing, sync, scanning, and product action surfaces. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| Field Companion | 33 | 13 |

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
