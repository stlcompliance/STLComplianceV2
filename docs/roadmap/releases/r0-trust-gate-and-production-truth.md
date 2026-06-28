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

Status: Product-by-product R0 pass completed in roadmap order. The suite may move to R1 stage work only with the deferred R0 blockers below carried as explicit release blockers; deferred products must not be described as production-trust-clear until those blockers close.

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
- LedgArr no longer falls back to an in-memory database outside Testing and no longer returns fake success for several scaffold-only finance endpoints.

Deferred R0 blockers:

- RecordArr: durable evidence/document store and tenant-scope work remains a production-trust blocker.
- SupplyArr: LoadArr-owned WMS/location/stock/reservation/ledger/movement ownership remains a blocker for physical inventory truth.
- LoadArr: authoritative movement/balance and receiving completion variants remain blocked where owner-backed traceability, quality/hold, and balance truth are incomplete.
- OrdArr: singleton process-local store remains a production-trust blocker.
- ReportArr: singleton scaffold BI store and launch-key-based source-product policy checks remain blockers for durable reporting, row/column security, lineage, schedules, exports, and retained outputs.
- Field Companion: mobile execution depends on NexArr and owning-product APIs for durable offline intents, submissions, sync outcomes, and final validation; legacy test compatibility rows should be retired during remaining NexArr launch cleanup.
- LedgArr: converted `501` finance placeholders remain blockers until implemented durably for dimension mappings, posting-rule management, financial-packet rejection, integration account mapping, AP disputes, AR credit memos/statements, and inventory valuation item/movement lists.
- NexArr: full `STLCompliance.NexArr.Auth.Tests` project still needs runtime investigation after prior full-suite timeouts; focused R0 coverage passed.

Tests run:

- Focused backend R0 tests across affected product auth/session, tenant settings, authorization, mobile, and finance slices.
- Focused frontend R0 tests across affected product session storage, API client normalization, shell/layout, app, offline queue, and settings slices.
- Known warnings remain in several .NET test runs: existing NuGet pruning warnings, EF Core version-conflict warnings, and existing xUnit analyzer warnings.

Stage result:

- R0 rollout bookkeeping is complete for every applicable product.
- R1 may begin only under the stage-gated rule, with no R2/R3 expansion and with the deferred R0 blockers preserved as release blockers rather than silently treated as done.
