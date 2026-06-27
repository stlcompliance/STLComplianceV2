# R7A — Customer master baseline

Provide customer accounts, contacts, locations, requirements, contracts, preferences, and eligibility before orders or dispatch consume customer truth.

| Field | Definition |
| --- | --- |
| Entry condition | StaffArr/RecordArr/Compliance Core foundations and user-facing CRM surfaces are durable enough for trusted customer onboarding. |
| Exit condition | Customer requirements can be queried by OrdArr, RoutArr, SupplyArr, AssurArr, ReportArr, and external portal workflows. |
| Total feature rows mapped here | 35 |
| Total workflow rows mapped here | 14 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| CustomArr | 70 | 16 | Customer accounts, contacts, locations, requirements, contracts, preferences, onboarding, eligibility, and CRM/customer portal context. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| CustomArr | 35 | 14 |

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
