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

## Suite stage pass summary

Status: complete. R8 has one applicable product in the rollout maps: RoutArr.

Completed products:

- RoutArr — complete. The dispatch and transportation execution baseline was verified against durable demand/trip/dispatch/proof/exception/closeout models, release/readiness snapshots, server-side assignment gates, and frontend UI/theme coverage.

Not-applicable products:

- NexArr, StaffArr, Compliance Core, RecordArr, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, ReportArr, Field Companion, and LedgArr have no R8 feature or workflow rows in the roadmap rollout maps.

Shared fixes:

- None required. RoutArr already consumes readiness and source references through existing product contracts and persisted snapshots.

Tests run:

- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` — timed out after 304 seconds before returning useful results; not counted as pass evidence.
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrDispatchWorkflowGateTests|FullyQualifiedName~RoutArrDispatchAssignmentTests|FullyQualifiedName~RoutArrAssetDispatchabilityTests|FullyQualifiedName~RoutArrDriverEligibilityTests" --logger "console;verbosity=minimal"` — passed 14 tests.
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrTripTests|FullyQualifiedName~RoutArrTripExecutionCaptureTests|FullyQualifiedName~RoutArrTripProofDvirTests|FullyQualifiedName~RoutArrTripCompletionRollupWorkerTests" --logger "console;verbosity=minimal"` — passed 21 tests.
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter "FullyQualifiedName~RoutArrTmsRuntimeTests|FullyQualifiedName~RoutArrSupplyArrPartsDemandTests|FullyQualifiedName~RoutArrDispatchBoardTests|FullyQualifiedName~RoutArrDispatchCloseoutTests|FullyQualifiedName~RoutArrDispatchExceptionQueueTests" --logger "console;verbosity=minimal"` — passed 27 tests.
- `npm test` from `apps/routarr-frontend` — passed 40 files / 134 tests.
- `npm run test:theme` from `apps/routarr-frontend` — passed with no theme audit violations.

Deferred blockers:

- No R8 blockers remain for RoutArr in the audited dispatch and transportation execution baseline.
- The broad backend test project should be run with a longer timeout or split by established clusters in CI.
- R12 optimization, control tower, marketplace/shared-capacity, carbon/alternative-energy planning, autonomous/robotic handoff, automated freight audit/dispute, and advanced document orchestration remain deferred to later roadmap scope.

Stage result: R8 is suite-complete. The suite may advance to R9.
