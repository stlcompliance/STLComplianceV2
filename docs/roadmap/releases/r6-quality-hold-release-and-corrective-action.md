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

## Stage pass summary - 2026-06-27

Status: Complete for the R6 suite gate, with explicit deferred blockers retained for later QMS depth. The suite may advance to R7A.

Completed products:

- AssurArr - clear for the durable quality hold, release, nonconformance, CAPA, audit/finding, supplier quality, complaint, status snapshot, and quality decision loop.

Not-applicable products: all other suite products for R6 under the roadmap rollout maps.

Deferred blockers:

- `AS-WF-010` quality change control remains target scope.
- `AS-WF-011` deviation and temporary concession remains target scope.
- `AS-WF-012` risk/FMEA review and control action remains target scope.
- `AS-WF-013` management quality review remains partial.
- `AS-WF-014` quality audit/evidence package remains partial.
- Rich downstream acknowledgement/unblock confirmations remain future depth where affected products must expose owner-backed contracts.
- RecordArr durable evidence package retention remains a suite blocker for production audit/evidence packages.

Shared fixes: none outside AssurArr.

Tests run:

- AssurArr backend exact tenant-scope regression passed 1 test.
- AssurArr backend full `STLCompliance.AssurArr.Api.Tests` passed 34 tests, and the current repo-state `AssurArrApiTests|AssurArrAuthorizationTests` rerun passed 26 tests.
- AssurArr frontend `npm test` passed 3 files / 7 tests, and the current repo-state client/app/session rerun passed 3 files / 8 tests.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)
