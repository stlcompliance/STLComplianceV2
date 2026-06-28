# R9 — Field Companion mobile execution

Put selected product actions in the field once owning APIs can enforce workflow, permissions, and idempotency.

| Field | Definition |
| --- | --- |
| Entry condition | Each mobile action has an owning product API, retry semantics, evidence rules, and clear blocked/degraded states. |
| Exit condition | Mobile/offline users can complete assigned work without Field Companion becoming a hidden source of truth. |
| Total feature rows mapped here | 33 |
| Total workflow rows mapped here | 13 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| Field Companion | 71 | 17 | Mobile assigned work, secure capture/upload, offline queueing, sync, scanning, and product action surfaces. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| Field Companion | 33 | 13 |

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

## Suite-stage R9 completion summary

Status: Complete. R9 is Field Companion-only, and Field Companion is clear for the stage.

Completed products:

- Field Companion — cleared R9 after auditing the mapped mobile execution rows, the mobile/offline constitution, Field Companion product docs, the NexArr-backed server test slice, and Field Companion frontend surfaces.

Not-applicable products:

- NexArr, StaffArr, Compliance Core, RecordArr, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, RoutArr, ReportArr, and LedgArr have no R9 roadmap rows. Their owning APIs remain dependencies for Field Companion mobile actions, but no product-stage R9 pass was started for them.

Shared fixes:

- No shared code changes were required.
- Field Companion frontend copy and tests were hardened to avoid exposing tenant plumbing or internal IDs in ordinary mobile surfaces while keeping recovery paths actionable.

Tests run:

- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~FieldCompanion" --logger "console;verbosity=minimal"` — passed 65 tests.
- `npm test -- ProfilePage.test.tsx FieldScanPanel.test.tsx OfflineQueuePanel.test.tsx` in `apps/fieldcompanion-frontend` — passed 3 files / 8 tests.
- `npm test -- ProfilePage.test.tsx HomePage.test.tsx FieldScanPanel.test.tsx OfflineQueuePanel.test.tsx SharedDeviceProtectionOverlay.test.tsx ProductWorkspaceLayout.test.tsx` in `apps/fieldcompanion-frontend` — passed 6 files / 15 tests.
- `npm test` in `apps/fieldcompanion-frontend` — passed 54 files / 152 tests.
- `npm run test:theme` in `apps/fieldcompanion-frontend` — no violations.

Deferred blockers:

- None for R9.
- R12 retains external capture links, advanced MAM/MDM integration, voice/glove and computer-vision workflows, advanced offline policy, geofencing/proximity, AR guidance, and expanded mobile observability.

Suite may advance to R10.
