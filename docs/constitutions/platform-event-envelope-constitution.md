# STL Compliance Platform Event Envelope Constitution

## 1. Purpose

This constitution standardizes the cross-product integration event envelope, durable outbox expectations, inbox/idempotency expectations, and event catalog naming rules.

## 2. Prime Directive

Events are facts. They are not commands.

Every product that publishes cross-product integration events must publish a tenant-scoped envelope from a durable outbox or equivalent reliable mechanism.

Every product that consumes cross-product integration events must process idempotently through an inbox, processed-event table, or equivalent mechanism.

## 3. Required Envelope Fields

Each integration event envelope must include:

- `eventId`
- `eventType`
- `productKey`
- `tenantId`
- `aggregateType`
- `aggregateId`
- `aggregateVersion` when available
- `occurredAt`
- `actorPersonId` when available
- `actorType`
- `sourceProductKey`
- `correlationId`
- `causationId`
- `idempotencyKey`
- `schemaVersion`
- `payload`
- visibility classification when available
- trace metadata when available

## 4. Actor Types

Allowed actor types are:

- `person`
- `service`
- `portalCustomer`
- `vendor`
- `system`
- `integration`

Human actors should use `actorPersonId` when available.

## 5. Event Naming

Event names must use:

`{productKey}.{domain}.{pastTenseFactOrStateChanged}`

Examples:

- `customarr.portalSubmission.created`
- `ordarr.order.accepted`
- `maintainarr.workOrder.scheduled`
- `loadarr.dockAppointment.rescheduled`

The product prefix must be one of the canonical lowercase product keys from `platform-product-key-naming-constitution.md`.

## 6. Payload Rules

Payloads must not include:

- secrets
- access tokens
- raw service tokens
- hidden prompts
- unrestricted sensitive notes
- cross-tenant data

Payloads should include stable identifiers, source references, plain summary fields, and enough context for consumers to decide whether to fetch from the owning product.

## 7. Publication Behavior

Products must emit events only after the owning transaction commits.

Failed publication must be retryable, observable, and recoverable.

Duplicate publication or duplicate consumption must not create duplicate records, handoffs, notifications, schedules, report rows, or external writebacks.

## 8. Inbox Behavior

Consumers must record at least:

- tenant ID
- source product
- source event ID
- event type
- idempotency key or equivalent
- outcome
- first processed time
- last processed time
- retry or failure state when applicable

Retries must be safe.

## 9. Event Catalog

The suite event catalog is reserved in shared contracts and documentation. Products should emit only events they actually own and implement.

Adding an event name does not grant permission to mutate another product's records.

## 10. Minimum Acceptable Implementation

An integration event flow is minimally acceptable when it has:

1. Canonical product-key prefix.
2. Required envelope fields.
3. Durable publication or equivalent reliability.
4. Idempotent consumption.
5. Tenant scope.
6. Schema version.
7. No secret payload fields.
8. Retry/failure visibility.
9. Source and aggregate identity.
10. Tests for duplicate handling.
