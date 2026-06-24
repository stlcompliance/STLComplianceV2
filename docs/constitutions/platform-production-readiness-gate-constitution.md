# STL Compliance Production Readiness Gate Constitution

## 1. Audit drivers

This constitution responds directly to SEC-001 through SEC-018, REL-001 through REL-007, CQ-001/CQ-005, FUNC-001, TEST-001, and the audit finding that several polished products were simulations rather than durable systems of record.

## 2. Prime directive

A product is not production-capable because its routes render or its happy path returns 200. It is production-capable only after it proves authorization, tenant isolation, durable state, truthful failure behavior, restart safety, contract correctness, and meaningful automated regression coverage.

## 3. Mandatory release gates

Every production product must prove:

- deny-by-default API authorization
- explicit tenant resolution from validated context
- action-level permissions and record scope
- durable persistence for every advertised system-of-record workflow
- no production-reachable fixture, singleton, no-op, or local-success fallback
- server-owned lifecycle transitions and concurrency handling
- idempotent retry behavior for retryable writes
- immutable or append-only audit attribution where required
- safe upload and evidence handling where files are accepted
- browser-session and SPA hardening
- clean-checkout build, migration, tests, theme audit, and browser smoke tests
- loading, empty, error, forbidden, conflict, stale, degraded, and partial-data UI states
- light and dark readability for every page/component state

## 4. Primary-record completeness

Every primary record owned by a product requires, where applicable:

- canonical list/queue page
- create page or guided workflow
- read-first detail page
- contextual drawer/peek
- explicit edit/lifecycle actions
- permissions and denial behavior
- history/activity and related records
- evidence/documents
- professional print/report output
- API, persistence, migration, and tests

A route stub, fixture list, local-only mutation, or unpersisted response does not satisfy the requirement.

## 5. Deployment blockers

Release is blocked when any of the following exists:

- anonymous domain route not explicitly intended for public intake
- hard-coded tenant or actor
- cross-tenant query path
- process-local mutable production store
- failed write displayed as success
- missing or fake migration/CI gate
- zero-test product reporting green
- refresh credential in JavaScript-readable persistence
- unbounded/unscanned upload
- theme-audit violation on reachable UI
- unavailable dependency represented as complete data

## 6. Evidence package

Each product release must generate a readiness record containing:

- endpoint authorization matrix result
- tenant-isolation negative-test result
- persistence/restart/multi-replica result
- migration and rollback/supersession result
- test inventory and counts by risk family
- theme/accessibility/visual smoke result
- route/page-archetype inventory
- known exceptions with owner and expiration

## 7. No waiver by appearance

A polished UI increases—not decreases—the need for proof because users reasonably assume visible operations are real. Warning banners do not make fake writes, global data, or non-durable state acceptable.
