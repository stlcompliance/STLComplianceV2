# R0 — Trust gate and production truth

Make every already-visible surface truthful, tenant-scoped, permissioned, durable, testable, readable in light/dark, and honest in failure.

| Field | Definition |
| --- | --- |
| Entry condition | Existing docs and code are known to contain uneven maturity across products. |
| Exit condition | No production route relies on local success, fixture truth, process-global stores, anonymous unsafe access, misleading launch errors, or untested tenant boundaries. |
| Total feature rows mapped here | 0 |
| Total workflow rows mapped here | 0 |

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

## Suite R0 pass summary

Status: Product-by-product R0 pass completed in roadmap order. The suite may move to R1 stage work, but the deferred R0 blockers below remain explicit release blockers and the affected products must not be described as production-trust-clear beyond their audited current slices until those blockers close.

Completed product passes:

- NexArr
- StaffArr
- Compliance Core
- RecordArr
- MaintainArr
- TrainArr
- SupplyArr
- LoadArr
- AssurArr
- CustomArr
- OrdArr
- RoutArr
- ReportArr
- Field Companion
- LedgArr

Not applicable products: None.

Shared fixes completed:

- Repeated product handoff/session paths now use fixed ordinary-suite launch catalogs rather than product-specific entitlement/access flags or claim-carried launch keys.
- Compliance Core remains outside ordinary tenant product launch lists while runtime/guidance paths remain available through owning workflow integrations.
- Field Companion session/profile launch lists are fixed ordinary-suite lists, with mobile-capable `fieldProductKeys` kept separate.
- Shared API host test runs now fall back to EF Core in-memory storage only in the `Testing` environment when no database URL is configured, which restores truthful endpoint-level verification without reintroducing non-Testing in-memory product truth.
- LedgArr no longer falls back to an in-memory database outside Testing and no longer returns fake success for several scaffold-only finance endpoints.

Deferred R0 blockers:

- SupplyArr: LoadArr-owned WMS/location/stock/reservation/ledger/movement ownership remains a blocker for physical inventory truth.
- LoadArr: authoritative movement/balance and receiving completion variants remain blocked where owner-backed traceability, quality/hold, and balance truth are incomplete.
- ReportArr: singleton scaffold BI store remains a blocker, and durable row/column/report security, source lineage, delegated action permissions, export security, and RecordArr-backed retained outputs are still incomplete for production reporting trust.
- StaffArr: full `STLCompliance.StaffArr.Auth.Tests` project still does not complete in the current repo state within a 20-minute local `dotnet test --no-build` window; focused R0 coverage passed and `StaffArrHandoffApiTests` now passes as a full class run.
- Field Companion: mobile execution depends on NexArr and owning-product APIs for durable offline intents, submissions, sync outcomes, and final validation; legacy test compatibility rows should be retired during remaining NexArr launch cleanup.
- Products clear in their audited current R0 slices with no active R0 blocker recorded: NexArr, Compliance Core, RecordArr, MaintainArr, TrainArr, AssurArr, CustomArr, OrdArr, RoutArr, and LedgArr.

Tests run:

- Focused backend R0 tests across affected product auth/session, tenant settings, authorization, mobile, and finance slices.
- Full `STLCompliance.NexArr.Auth.Tests` suite now passes in the current repo state (421 tests, 10m 57s), clearing the prior NexArr runtime-investigation note.
- `STLCompliance.StaffArr.Auth.Tests` still times out as a full-project run in the current repo state, but `StaffArrHandoffApiTests` now passes as a full class run (56 tests, 7m 16s), narrowing the remaining StaffArr runtime investigation.
- Compliance Core current-slice auth and platform-admin/runtime boundary verification remains clear in the current repo state.
- RecordArr current-slice R0 verification now passes in the current repo state: `STLCompliance.RecordArr.Auth.Tests` passed 4 tests, the focused OpenAPI/backend durability cluster (`RecordArrStoreTests`, `RecordArrIntegrationEndpointTests`, `RecordArrPrintProviderTests`) passed 93 tests, and the RecordArr frontend `sessionStorage.test.ts` plus `App.test.tsx` suite passed 12 tests.
- MaintainArr's focused R0 backend verification still passes in the current repo state: `MaintainArrHandoffApiTests` passed 10 tests in 2m 6s, and a permission/tenant-isolation/no-persist cluster across readiness, maintenance history, notifications, and bulk import passed 34 tests in 3m 37s.
- TrainArr's focused R0 verification still passes in the current repo state: `TrainArrHandoffApiTests` passed 4 tests in 32s, a TrainArr qualification/evidence/signoff cluster passed 24 tests in 2m 7s, and the TrainArr frontend `client.test.ts` plus `ProductWorkspaceLayout.test.tsx` suite passed 6 tests.
- SupplyArr's focused R0 verification still passes in the current repo state: the handoff/session/me/bootstrap trust-gate cluster passed 5 tests in 49s, and the SupplyArr frontend `client.test.ts`, `ProductWorkspaceLayout.test.tsx`, and `sessionStorage.test.ts` suite passed 47 tests across 3 files in 3.05s. A full-class `SupplyArrHandoffApiTests` rerun still exceeded the local command timeout, so the focused cluster remains the documented R0 verification slice.
- LoadArr's focused R0 verification still passes in the current repo state: the auth/session/tenant-settings trust-gate cluster passed 10 tests in 12s, and the LoadArr frontend `client.test.ts`, `mutationMessages.test.ts`, and `App.test.tsx` suite passed 51 tests across 3 files in 24.56s.
- AssurArr's focused R0 verification still passes in the current repo state: the auth/session/startup-guard cluster passed 13 tests in 14s, and the AssurArr frontend `client.test.ts`, `App.test.tsx`, and `sessionStorage.test.ts` suite passed 8 tests across 3 files in 4.71s.
- CustomArr's focused R0 verification still passes in the current repo state: the auth/session/startup-guard and tenant-scoped workspace/settings cluster passed 18 tests in 7s, and the CustomArr frontend `App.test.tsx` plus `sessionStorage.test.ts` suite passed 5 tests across 2 files in 16.50s.
- OrdArr current-slice R0 verification now passes in the current repo state: the auth/store cluster passed 14 tests in 3s, and the OrdArr frontend `App.test.tsx` plus `sessionStorage.test.ts` suite passed 11 tests in 13.87s.
- RoutArr current-slice R0 verification remains clear in the current repo state: the handoff/session/backend slice passed 6 tests, and the RoutArr frontend `ProductWorkspaceLayout.test.tsx` plus `sessionStorage.test.ts` suite passed 6 tests.
- ReportArr current-slice R0 verification still truthfully blocks on the scaffold BI engine but the auth/session slice remains covered: `ReportArrAuthEndpointsTests` passed 7 tests in 5s, and the ReportArr frontend `App.test.tsx` plus `sessionStorage.test.ts` suite passed 6 tests in 3.13s.
- Field Companion current-slice R0 verification remains clear in the current repo state: `NexArrFieldCompanionFieldInboxTests` passed 7 tests, and the Field Companion frontend client/session/offline/workspace cluster passed 21 tests.
- LedgArr current-slice R0 verification now passes in the current repo state: the auth/settings/former-placeholder backend cluster passed 11 tests in 5s, and the LedgArr frontend `App.test.tsx` plus `sessionStorage.test.ts` suite passed 6 tests in 14.81s.
- Focused frontend R0 tests across affected product session storage, API client normalization, shell/layout, app, offline queue, and settings slices.
- Known warnings remain in several .NET test runs: existing NuGet pruning warnings, EF Core version-conflict warnings, and existing xUnit analyzer warnings.

Stage result:

- R0 rollout bookkeeping is complete for every applicable product.
- No products are marked not applicable for R0.
- R1 may begin only under the stage-gated rule, starting again from the first product in rollout order, with no R2/R3 expansion and with the deferred R0 blockers preserved as release blockers rather than silently treated as done.
