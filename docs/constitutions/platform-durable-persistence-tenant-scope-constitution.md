# STL Compliance Durable Persistence and Tenant Scope Constitution

## 1. Audit drivers

SEC-001 through SEC-004 and FUNC-001 exposed hard-coded tenants, global singleton stores, missing tenant fields, discarded mutations, and restart data loss.

## 2. Prime directive

Every tenant-owned business fact must be durably stored and tenant-scoped at every boundary. Tenant context comes from validated identity or service context, never from an ordinary request body.

## 3. Required tenant scope

Tenant scope applies to:

- aggregates, children, versions, timelines, approvals, holds, and comments
- read models, dashboards, caches, search indexes, exports, and object keys
- schedules, jobs, worker leases, inbox/outbox rows, and idempotency records
- audit, access logs, external shares, notifications, and generated evidence
- uniqueness constraints and database indexes

A child record may inherit tenant through a constrained parent relation, but queries must still prove tenant ownership before access.

## 4. Persistence rule

Production workflows may not use mutable singleton lists, static dictionaries, browser-local records, seeded fixtures, or process memory as the system of record. In-memory state is allowed only for tests, disposable caches with authoritative backing, or explicitly isolated demo environments.

## 5. Query rule

Every read and mutation must include tenant scope before record scope. Looking up by globally supplied ID and checking later is insufficient when it permits timing, existence, or error-shape inference.

## 6. Cross-product references

Products store stable owner references and necessary display/audit snapshots. They do not copy foreign domain truth into local shadow masters. No cross-database foreign keys or direct joins are allowed.

## 7. Restart and replica safety

State must survive process restart and behave consistently with multiple API/worker replicas. Schedules and jobs require durable leases; retries require durable idempotency; event publication requires outbox/inbox patterns where consistency matters.

## 8. Required tests

For every aggregate family:

- two-tenant colliding-ID/name tests
- list/detail/mutation cross-tenant denial
- restart persistence test
- multi-replica/idempotency test
- tenant-scoped unique constraint test
- cache/index/object-storage key isolation test

## 9. UI consequence

The UI must never imply that local optimistic or cached state is final. Pending, offline, stale, and unsynchronized records must be visibly distinct until durable server confirmation.
