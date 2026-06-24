# Product Production-Readiness Scorecard Template

A product is release-ready only when every mandatory row is **Pass** with linked evidence. “Partial” is not release acceptance.

| Gate | Status | Required evidence |
|---|---|---|
| Ownership boundaries |  | scope doc and no shadow ownership |
| Durable storage |  | schema/migrations, restart and multi-replica tests |
| Tenant isolation |  | query rules, indexes, two-tenant collision tests |
| Endpoint authorization |  | complete route map and 401/403/cross-tenant tests |
| Actor attribution |  | claim/service delegation source and spoof tests |
| State machines/concurrency |  | transition table, concurrency/idempotency tests |
| Cross-product contracts |  | API/event schemas, owner-backed refs, degraded behavior |
| Fixture/no-op removal |  | production scan and real-write tests |
| Error truthfulness |  | failed-write, partial, stale, conflict and retry UI tests |
| Primary-record page coverage |  | list/drawer/detail/create-edit/history/evidence/print map |
| Unified shell/navigation |  | route/breadcrumb/sidebar/mobile verification |
| Light/dark consistency |  | token audit and visual smoke |
| Accessibility |  | keyboard/focus/name/contrast/responsive checks |
| Upload/file safety |  | limits/quarantine/scanning tests where applicable |
| Background workers |  | lease/retry/idempotency/recovery tests |
| CI inclusion |  | clean-checkout build/test/migration/browser jobs |
| Observability/support |  | health, correlation, safe diagnostics and runbook |
| Documentation |  | manifests, constitutions, route/event/workflow updates |

## Page evidence minimum

For every primary page archetype capture light and dark evidence at realistic density, plus at least loading, empty/no-results, forbidden, degraded, error, and conflict states when relevant. Screenshots do not replace automated assertions.

## Release decision

- **Pass:** all mandatory gates pass.
- **Internal testing only:** no critical tenant/security/durability/truthfulness issue remains, but noncritical evidence is incomplete.
- **Blocked:** any unsafe anonymous/cross-tenant path, process-local production truth, fake/no-op success, broken CI gate, insecure credential/session storage, or untested critical transition remains.
