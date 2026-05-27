# Design Decision Log

## ARR Means Adaptive Risk Reduction

Reason: the suite is broader than maintenance. It covers workforce, training, assets, dispatch, supply, and compliance authority.

## Full V1 Shape

Every product ships as a real API, worker, database, and UI surface in V1.

Reason: this prevents accidental domain absorption and keeps ownership boundaries real from day one.

## NexArr as Control Plane

Reason: login, tenants, entitlement, licensing, launch, and service authority must be consistent across the suite.

## StaffArr as Workforce Backbone

Reason: all products need one place to ask who a person is, where they belong, what they can do, and whether they are ready.

## Compliance Core as Authority Layer

Reason: products need vocabulary, keys, mappings, and rule context without becoming separate rule systems.

## Separate Product Databases

Reason: database separation protects ownership, supports future hybrid data planes, and avoids hidden coupling.

## No Cross-Product Foreign Keys

Reason: API and event contracts must connect products, not database constraints across service boundaries.

## Render First

Reason: static sites, web services, background workers, PostgreSQL, Redis-compatible Key Value, and render.yaml match the suite deployment shape.

## Docker for APIs and Workers

Reason: Docker gives predictable .NET 10 service packaging across APIs and workers.

## React + Vite Frontend

Reason: static deployment, fast development, and a strong fit for the suite shell and marketing site.

## Lucide React Icons

Reason: consistent stroke icon language and clean React imports.

## MaintainArr Does Not Own Procurement

Reason: work orders need parts in context, but vendors, PR/PO, receiving, inventory, pricing, and lead times belong to SupplyArr.

## RoutArr Does Not Own Drivers as People

Reason: driver is a StaffArr person in a routing context. Qualification and readiness come from StaffArr and TrainArr.

## TrainArr Publishes to StaffArr

Reason: TrainArr owns proof of training, while StaffArr owns person readiness and personnel history.

## Compliance Core Does Not Replace Product Workflows

Reason: Compliance Core provides rule context and results; products own operational decisions.
