# CustomArr Portal Order Handoff

## Purpose

This document defines how a customer-facing CustomArr portal submission becomes an OrdArr order/request without making CustomArr the order owner.

## Ownership

CustomArr owns:

- customer accounts
- customer contacts
- customer portals
- portal sessions
- raw portal submissions
- customer-facing status surfaces

OrdArr owns:

- order/request lifecycle
- order/request status
- requested and promised windows
- change and cancellation decisions
- downstream fulfillment orchestration

Execution products own their own demand and schedules after OrdArr requests or accepts downstream work.

## Flow

1. Customer submits a portal form in CustomArr.
2. CustomArr validates the portal form and stores the raw submission.
3. CustomArr maps the submission to a normalized order request.
4. CustomArr emits `customarr.portalSubmission.created`.
5. CustomArr calls OrdArr create order/request with a tenant-scoped idempotency key.
6. OrdArr creates a canonical order request or rejects the request.
7. OrdArr emits `ordarr.order.requested` or `ordarr.order.created`.
8. OrdArr accepts, rejects, or holds the order based on customer, service, requested dates, terms, line items, and required facts.
9. If accepted, OrdArr emits `ordarr.order.accepted`.
10. OrdArr creates downstream fulfillment handoffs or demand requests through owning product APIs.
11. The shared Planning Board displays product-owned unscheduled demand.
12. Drag/drop scheduling calls the owning execution product.
13. CustomArr portal displays customer-facing status from OrdArr and downstream projections, not guessed CustomArr state.

## Required Separation

Requested window, promised window, and scheduled execution must stay separate.

Portal submission status is not order status.

Order acceptance is not dispatch scheduling.

CustomArr must not directly create RoutArr trips, LoadArr dock appointments, MaintainArr work orders, TrainArr assignments, AssurArr quality checks, or SupplyArr purchase orders for ordinary order flow.

## Customer Change Or Cancellation

CustomArr may capture an external change or cancellation request, but OrdArr owns the canonical change or cancellation state.

OrdArr emits:

- `ordarr.order.changeRequested`
- `ordarr.order.changed`
- `ordarr.order.changeRejected`
- `ordarr.order.cancelRequested`
- `ordarr.order.cancelled`

Downstream products decide whether to cancel, reschedule, hold, or keep their own work. Completed work is not erased.
