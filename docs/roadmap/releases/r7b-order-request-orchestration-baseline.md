# R7B — Order/request orchestration baseline

Turn customer/internal demand into owned execution handoffs, exceptions, completion packets, and bill-ready intent.

| Field | Definition |
| --- | --- |
| Entry condition | CustomArr customer truth and execution-product readiness contracts exist. |
| Exit condition | An order/request can prove who requested work, why it exists, what products own each step, what is blocked, and what completed. |
| Total feature rows mapped here | 31 |
| Total workflow rows mapped here | 13 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| OrdArr | 66 | 15 | Orders, requests, order lifecycle, triage, handoffs, exception coordination, completion packets, and bill-ready intent. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| OrdArr | 31 | 13 |

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

## Suite stage pass summary

Status: complete. R7B has one applicable product in the rollout maps: OrdArr.

Completed products:

- OrdArr — complete. The order/request orchestration baseline now persists tenant-scoped order snapshots and idempotency records through EF-backed tables and migrations, replacing the prior singleton process-local order store for this release slice.

Not-applicable products:

- NexArr, StaffArr, Compliance Core, RecordArr, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, RoutArr, ReportArr, Field Companion, and LedgArr have no R7B feature or workflow rows in the roadmap rollout maps.

Shared fixes:

- No shared platform code was required.
- The existing OrdArr/CustomArr handoff smoke test was updated to construct both product stores with tenant-aware EF contexts so the durable-store path is tested without changing product ownership boundaries.

Tests run:

- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.OrdArr.Auth.Tests.OrdArrStoreTests.Orders_and_idempotency_survive_store_recreation" --logger "console;verbosity=minimal"` — passed 1 test.
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` — passed 14 tests.
- `npm test` from `apps/ordarr-frontend` — passed 2 files / 10 tests.
- `npm run test:theme` from `apps/ordarr-frontend` — passed with no theme audit violations.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName~OrdArrCustomArrHandoffTests" --logger "console;verbosity=minimal"` — passed 2 tests.
- Current repo-state reruns also passed: the focused OrdArr auth/store backend cluster passed 14 tests, the OrdArr frontend app/session slice passed 2 files / 11 tests, and the OrdArr/CustomArr handoff smoke passed 2 tests.

Deferred blockers:

- No R7B blockers remain for OrdArr in the audited order/request orchestration baseline.
- Durable event outbox delivery/reconciliation worker depth, advanced promising, downstream change compensation, partial/backorder decision support, exception control tower, transportation execution, and R12 portal/EDI/marketplace/import/export/AI/category-depth work remain deferred to their owning later stages.

Stage result: R7B is suite-complete. The suite may advance to R8.
