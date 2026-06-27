# R8 — Dispatch and transportation execution

Route and dispatch work only after driver, equipment, customer, order, inventory, and compliance readiness are explainable.

| Field | Definition |
| --- | --- |
| Entry condition | Orders, customer requirements, asset readiness, training qualification, and inventory/dock context are available. |
| Exit condition | A dispatch can be planned, assigned, executed, excepted, completed, and traced back to source demand and readiness snapshots. |
| Total feature rows mapped here | 38 |
| Total workflow rows mapped here | 15 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| RoutArr | 73 | 15 | Transportation demand, dispatch, routes, trips, stops, driver/equipment snapshots, exceptions, proof, dock visibility, and freight packets. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| RoutArr | 38 | 15 |

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
