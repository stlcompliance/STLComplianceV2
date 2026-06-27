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

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
