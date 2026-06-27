# Release Gates and Acceptance Evidence

A release train may not be called complete until the gates below are satisfied for the surfaces, APIs, workflows, and cross-product contracts included in that train.

## Universal R0 gate

| Gate | Required proof |
| --- | --- |
| Tenant isolation | Cross-tenant read/write denial tests for every unsafe and primary read path. |
| Authorization | Server-side permission checks for every unsafe action; no UI-only protection. |
| Durable state | Restart-survival tests for primary records, workflow state, evidence refs, and outbox/inbox records. |
| No production fixtures | Fixtures, demo stores, no-op writes, process-global collections, and local-success fallbacks are blocked from production paths. |
| Idempotency and concurrency | Unsafe actions have idempotency keys or equivalent retry safety, clear conflict behavior, and no duplicate side effects. |
| Error truthfulness | Errors preserve user work, explain what happened, and never imply success when the server rejected or failed the action. |
| Source-of-truth boundary | Cross-product data comes from owner APIs/events/snapshots; no cross-DB joins or local fake owner tables. |
| Audit and evidence | Actor, tenant, source record, event, evidence, status, and reason are traceable. |
| UI consistency | Shared shell/page archetypes, readable light/dark states, no dev labels, no unnecessary internal IDs, and no walls of text. |
| Print/report readiness | Printable pages render as professional report-style documents without the app shell when applicable. |

## Page and workflow gate

Every primary record introduced or relied upon by a release must have the applicable page archetypes: index/list, create, drawer/peek, detail, settings/preferences where relevant, empty/loading/error/degraded states, and print/export behavior where relevant.

Every workflow introduced or relied upon by a release must define trigger, owner at each step, state path, blockers, alternate paths, recovery, permissions, APIs/events, durable state, evidence, user-facing labels, and ReportArr/read-model effect.

## Cross-product contract gate

A release cannot depend on another product by copying its truth. It must use one of these patterns:

- synchronous owner API lookup;
- event plus reference snapshot;
- explicit handoff record;
- RecordArr evidence reference;
- Compliance Core evaluation payload;
- ReportArr read model derived from source events.

## Pull-forward gate

A later-stage feature can be included early only when it is needed for the vertical slice and its owner boundary is respected. The feature remains in the inventory, but the release note must state why it was pulled forward.
