# R5 — Procure, receive, put away, reserve, and issue

Connect maintenance and operating demand to procurement expectations, receiving, putaway, reservations, issues, and traceable inventory evidence.

| Field | Definition |
| --- | --- |
| Entry condition | Maintenace parts demand, StaffArr locations, RecordArr evidence, and platform reference data are available. |
| Exit condition | A part can be requested, ordered, received, inspected/excepted if needed, put away, reserved, issued, consumed, and traced. |
| Total feature rows mapped here | 73 |
| Total workflow rows mapped here | 29 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| SupplyArr | 72 | 16 | Suppliers, vendors, procurement expectations, purchase requests/orders, sourcing, pricing, and lead-time context. |
| LoadArr | 71 | 15 | Receiving, putaway, locations-as-references, item balances, stock ledger, reservations, picks, issues, transfers, counts, and discrepancies. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| SupplyArr | 37 | 16 |
| LoadArr | 36 | 13 |

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
