# Suite Scheduling And Created Events Implementation Notes

Date: 2026-06-16

## Constitution Read

Applicable constitutions were read before implementation:

- `docs/constitutions/ownership.md`
- `docs/constitutions/ui.md`
- `docs/constitutions/platform-api-integration-constitution.md`
- `docs/constitutions/platform-events-handoffs-readmodels-constitution.md`
- `docs/constitutions/platform-product-key-naming-constitution.md`
- `docs/constitutions/platform-list-board-queue-constitution.md`
- `docs/constitutions/platform-permission-action-matrix-constitution.md`
- `docs/constitutions/platform-security-tenancy-authority-constitution.md`
- `docs/constitutions/platform-external-portal-access-constitution.md`
- `docs/constitutions/platform-reporting-metrics-provenance-constitution.md`
- `docs/constitutions/platform-mobile-offline-capture-sync-constitution.md`
- `docs/constitutions/platform-workflow-approval-assignment-escalation-constitution.md`

## Current State Inventory

- Product APIs exist for AssurArr, Compliance Core, CustomArr, LoadArr, MaintainArr, NexArr, OrdArr, RecordArr, ReportArr, RoutArr, StaffArr, SupplyArr, and TrainArr.
- MaintainArr has the most mature existing pattern for this work: EF-owned domain records, product-local authorization, an outbox table, inbound event/idempotency records, audit events, worker processing endpoints, StaffArr site/person references, TrainArr qualification checks, Compliance Core gate clients, and work orders with planned windows and technician assignment fields.
- OrdArr and CustomArr currently use lightweight in-memory workspace stores. OrdArr already requires an `Idempotency-Key` header for create order/request and enforces CustomArr customer references. CustomArr owns customer context but has no portal submission model yet.
- The shared frontend package `packages/shared-ui` is used by product frontends through TypeScript path aliases. MaintainArr frontend already has work order pages and navigation but no shared scheduling board component.
- Existing event names in MaintainArr include product-local event kinds such as `work_order.created`; the requested suite contract requires cross-product event type names such as `maintainarr.workOrder.created`.

## Ownership Decisions

- No SchedulerArr product is added.
- Scheduling remains a shared interaction pattern and display contract, not a source-of-truth database.
- MaintainArr is the first fully wired scheduling owner because it already owns work orders and has durable outbox/inbox infrastructure.
- CustomArr portal order submission is implemented as CustomArr intake plus explicit handoff to OrdArr create order/request. CustomArr does not create RoutArr, LoadArr, MaintainArr, or other downstream execution records.
- OrdArr remains the owner of order/request lifecycle and downstream fulfillment orchestration. It reserves downstream demand references as product-owned handoffs rather than execution schedules.
- Product keys and permissions use lowercase canonical keys. Display names remain human friendly.

## Known Conflicts Resolved

- Existing MaintainArr outbox event kind strings are product-local and do not include product prefixes. The implementation keeps those as internal event kinds for compatibility with the existing outbox worker while adding a suite event type mapping and shared event envelope contract for cross-product publication.
- Existing MaintainArr `ScheduleDraftAsync` only changes draft status. The product-local scheduling API adds semantic scheduling endpoints that validate, update MaintainArr-owned fields, and emit scheduling-specific MaintainArr events.

## Partial Product Coverage

- MaintainArr receives functional scheduling endpoints and UI adapter coverage.
- OrdArr and CustomArr receive functional store/API coverage for the customer portal to order handoff.
- Other product scheduling ownership is documented and reserved in shared contracts/catalogs. Product-local scheduling endpoints should be added product by product using the shared contract and without centralizing writes.
