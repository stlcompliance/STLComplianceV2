# STL Compliance State Machine, Idempotency, and Concurrency Constitution

## 1. Audit drivers

The audit found non-durable order/WMS/report flows, unsafe refresh rotation, and workflow behavior enforced primarily in frontends.

## 2. Prime directive

Business lifecycle truth is enforced by the owning product on the server. Retried requests and concurrent users must not duplicate, regress, or silently overwrite that truth.

## 3. State machines

Every lifecycle-changing aggregate defines:

- canonical states
- allowed transitions
- transition permission
- required reason/evidence/approval
- invariant checks
- side effects/events
- terminal and reopening rules
- conflict behavior

Frontends may guide but may not be the only enforcement.

## 4. Idempotency

Retryable creates, postings, handoffs, inventory moves, payments, captures, imports, report runs, and event consumers use durable idempotency. Keys are scoped by tenant and operation, persisted with request hash/result, and protected by a unique constraint.

## 5. Concurrency

Mutable aggregates use version/row tokens or equivalent optimistic concurrency. A stale update returns a conflict with enough safe information to refresh, compare, and preserve unsaved user work.

## 6. Transactions and events

State change, audit/timeline entry, ledger effect, and outbox record are committed atomically where the workflow requires them. Consumers use durable inbox/deduplication.

## 7. Reversals

Financial, inventory, evidence, approval, and compliance-significant history is corrected through reversal, supersession, or explicit amendment rather than destructive overwrite.

## 8. Tests

Each state machine requires happy, invalid, unauthorized, duplicate, concurrent, retry, restart, and event-redelivery tests.
