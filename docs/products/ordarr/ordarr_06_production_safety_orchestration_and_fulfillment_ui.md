# OrdArr — Production Safety, Orchestration, and Fulfillment UI

## Audit mandate

Orders, lines, holds, returns, handoffs, timelines, completion packets, and finance contributions must be tenant-scoped durable records. Remove singleton/global stores and add tenant fields and constraints to every aggregate.

## Lifecycle

Use a server-owned order/request state machine with explicit submit, triage, accept, hold, release, cancel, fulfill, partial fulfill, complete, return, and close rules. Invalid transitions, stale updates, duplicate idempotency keys, and unauthorized actions are rejected.

## Ownership

CustomArr owns customer truth; SupplyArr owns procurement; LoadArr owns warehouse execution; RoutArr owns transportation; AssurArr owns quality decisions; MaintainArr owns service/work execution; LedgArr owns financial records. OrdArr owns orchestration, customer/request snapshots, handoff correlation, completion criteria, and consolidated status.

## Pages

Provide Orders/Requests list, triage queue, create wizard, drawer, detail with Overview/Lines/Fulfillment/Holds/Returns/Documents/History, and completion review. Every cross-product stage shows accepted, pending, blocked, failed, or complete truthfully.

## Events and workers

Persist outbox/inbox, handoff attempts, correlation, response, retries, and dead-letter/review state. Completion and finance packets are durable versioned contributions.
