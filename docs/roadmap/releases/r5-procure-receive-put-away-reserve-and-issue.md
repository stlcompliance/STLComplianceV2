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

## Stage pass summary - 2026-06-27

Status: Complete for the R5 suite gate, with explicit deferred blockers retained for future LoadArr/SupplyArr depth work. The suite may advance to R6.

Completed products:

- SupplyArr - clear for the SupplyArr-owned commercial/procurement R5 slice. Purchasing, supplier/vendor, procurement exception, reporting, reservation support, integration event, and frontend coverage passed while preserving LoadArr ownership of physical inventory execution.
- LoadArr - clear for the implemented clean receiving and traceability slice. LoadArr now commits a clean single-line receipt to durable tenant-scoped origin event, movement, inventory balance, and putaway task records, exposes those records through inventory/task/history/ledger reads, and keeps broader incomplete warehouse workflows truth-gated.

Not-applicable products: all other suite products for R5 under the roadmap rollout maps.

Deferred blockers:

- SupplyArr `SU-CUR-016` legacy WMS tables remain boundary debt and must not become canonical LoadArr physical inventory truth.
- SupplyArr `SU-WF-008`, `SU-WF-011`, `SU-WF-012`, and `SU-WF-013` remain partial where they require LoadArr physical receipt/disposition, AssurArr quality decisions, supplier performance, SCAR, or contract lifecycle depth outside the SupplyArr R5 commercial slice.
- LoadArr transfer completion, reservation/allocation/issue, pick/pack/stage/ship, replenishment, truck stock, kit, count, adjustment, hold/quarantine, unexplained inventory, returns, expected receipt/ASN, dock appointment, setup read-model, and integration synchronization workflows remain gated until authoritative movement, balance, audit, and owner-backed synchronization support exists.
- LoadArr receiving variants requiring inspection, discrepancy handling, richer lot/serial/SDS semantics, AssurArr quality disposition, or RecordArr durable evidence retention remain gated.
- RecordArr durable file retention remains a suite blocker for production evidence packages.

Shared fixes:

- SupplyArr mapped the stock reservation and report-index endpoint groups and tightened reference integration authorization for normal tenant users.
- LoadArr exposed already-committed durable operational records through tenant-scoped read APIs instead of leaving clean receiving outputs invisible behind blanket read-model-unavailable responses.

Tests run:

- SupplyArr backend: focused R5/auth/report/support clusters passed 39, 28, 33, 6, and 65 tests; current repo-state reruns also passed a 3-test support/boundary cluster and a 66-test broader support/boundary cluster.
- SupplyArr frontend: `npm test` passed 54 files / 151 tests, and the current repo-state client/layout/session slice passed 3 files / 53 tests.
- LoadArr backend: exact receiving trace test passed 1 test; full `STLCompliance.LoadArr.Auth.Tests` passed 111 tests; current repo-state reruns also passed a 5-test receiving/idempotency/history truth cluster.
- LoadArr frontend: `npm test` passed 7 files / 60 tests, and the current repo-state client/mutation/app slice passed 3 files / 51 tests.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
