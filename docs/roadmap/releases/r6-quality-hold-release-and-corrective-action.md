# R6 — Quality hold, release, and corrective action

Allow quality decisions to block or release assets, inventory, suppliers, orders, and records without taking over their source truth.

| Field | Definition |
| --- | --- |
| Entry condition | Affected products expose holdable/releasable references and evidence package hooks. |
| Exit condition | Quality holds and CAPA are permissioned, evidenced, auditable, and visibly block downstream operations until resolved. |
| Total feature rows mapped here | 33 |
| Total workflow rows mapped here | 14 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| AssurArr | 68 | 14 | Nonconformance, quality holds, releases, CAPA, audits, findings, complaints, supplier quality, and quality status. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| AssurArr | 33 | 14 |

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
