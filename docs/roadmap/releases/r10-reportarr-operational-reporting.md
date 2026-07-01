# R10 — ReportArr operational reporting

Turn accumulated source events, evidence, and workflow history into reports without mutating source products.

| Field | Definition |
| --- | --- |
| Entry condition | Enough products emit source refs, events, evidence refs, status history, and read-model contracts. |
| Exit condition | Reports are exportable, schedulable, provenance-aware, and can store audit-ready outputs in RecordArr. |
| Total feature rows mapped here | 33 |
| Total workflow rows mapped here | 13 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| ReportArr | 68 | 15 | Read models, dashboards, metrics, scheduled reports, exports, provenance drillbacks, and audit-ready report outputs. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| ReportArr | 33 | 13 |

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

## Suite-stage R10 completion summary

Status: Complete. R10 is ReportArr-only, and ReportArr is clear for the stage with deferred durable-BI-engine blockers documented in the product rollout notes.

Completed products:

- ReportArr — cleared R10 after auditing the mapped operational reporting rows, ReportArr product docs, the report/print/export constitution, backend source-reporting permission behavior, frontend reporting shell/tests, theme audit, and ReportArr OpenAPI/print-provider tests.

Not-applicable products:

- NexArr, StaffArr, Compliance Core, RecordArr, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, RoutArr, Field Companion, and LedgArr have no R10 roadmap rows. Their source events/evidence/read-model contracts remain upstream dependencies for ReportArr, but no product-stage R10 pass was started for them.

Shared fixes:

- No shared code changes were required.
- ReportArr source-product reporting access was tightened so ordinary fixed-suite product launch keys do not grant analytics visibility.

Tests run:

- `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` — passed 7 tests.
- `npm test` in `apps/reportarr-frontend` — passed 3 files / 8 tests.
- `npm run test:theme` in `apps/reportarr-frontend` — no violations.
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter "FullyQualifiedName~ReportArr" --logger "console;verbosity=minimal"` — passed 48 tests.
- Current repo-state reruns also passed: `ReportArrAuthEndpointsTests` passed 7 tests, the ReportArr frontend app/session slice passed 2 files / 6 tests, and the focused ReportArr OpenAPI suite passed 48 tests.

Deferred blockers:

- ReportArr still needs a normalized durable BI storage migration for connectors, events, semantic definitions, runs, schedules, exports, lineage, metrics, and audit packages.
- Full warehouse-grade replay/rebuild orchestration, delivery reconciliation, retained-output lifecycle state, fine-grained row/column sensitivity labels, external portal scopes, and delegated StaffArr/NexArr action permissions remain deferred.
- R12 retains advanced BI expansion: natural-language analysis, advanced semantic layer/lakehouse orchestration, forecasting/anomaly depth, embedded analytics SDK, scenario analysis, data catalog, privacy-preserving analytics, streaming intelligence, and advanced model governance.

Suite may advance to R11.
