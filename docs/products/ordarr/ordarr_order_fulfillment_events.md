# OrdArr Order Fulfillment Events

## Purpose

This document defines how OrdArr uses events and handoffs to coordinate fulfillment while preserving execution-product ownership.

## Ownership

OrdArr owns why work is happening: the order/request, lifecycle, requested windows, promised windows, customer-facing status, fulfillment state, and closeout packets.

Execution products own how work is performed:

- RoutArr owns transport demand, trips, dispatch, driver/equipment assignment, stops, and transportation execution.
- LoadArr owns dock appointments, receiving, staging, putaway, warehouse tasks, and inventory execution.
- SupplyArr owns vendor, supplier, procurement, purchase order, and material sourcing work.
- MaintainArr owns readiness checks, maintenance work orders, inspections, defects, repairs, and asset readiness.
- AssurArr owns quality checks, nonconformance, reviews, and corrective actions.
- TrainArr owns training assignments, evaluations, certificates, and retraining.

## Canonical Order Events

OrdArr reserves and emits these order events where implemented:

- `ordarr.order.requested`
- `ordarr.order.created`
- `ordarr.order.accepted`
- `ordarr.order.rejected`
- `ordarr.order.held`
- `ordarr.order.released`
- `ordarr.order.changed`
- `ordarr.order.changeRequested`
- `ordarr.order.changeRejected`
- `ordarr.order.cancelRequested`
- `ordarr.order.cancelled`
- `ordarr.order.promisedWindowSet`
- `ordarr.order.fulfillmentRequested`
- `ordarr.order.fulfillmentBlocked`
- `ordarr.order.fulfillmentReleased`
- `ordarr.order.closed`
- `ordarr.order.reopened`
- `ordarr.orderLine.created`
- `ordarr.orderLine.changed`
- `ordarr.orderLine.cancelled`

## Downstream Demand Pattern

When an order is accepted, OrdArr determines needed fulfillment lanes and requests work through owning product APIs or explicit handoffs.

OrdArr may request:

- RoutArr transport demand
- LoadArr warehouse or dock demand
- SupplyArr procurement or vendor confirmation demand
- MaintainArr readiness or maintenance demand
- AssurArr quality review demand
- RecordArr document/evidence linking
- Compliance Core evaluations

OrdArr must not directly assign drivers, docks, mechanics, trainers, inspectors, or warehouse teams.

## Cancellation And Change

OrdArr owns the change or cancellation decision. Downstream products receive commands, handoffs, or events and apply their own status rules.

Rules:

- Already completed downstream work is preserved.
- Scheduled future work is cancelled or rescheduled through the owning product API.
- Audit and reporting history is preserved.
- Customer-facing status is derived from OrdArr and downstream projections.
