# Maintenance Manager Guide

## What This Role Does
A maintenance manager plans and supervises maintenance work, asset readiness, defects, inspections, PM programs, parts demand, downtime, and maintenance evidence in MaintainArr.

## What This Role Can Usually Access
- MaintainArr Asset registry, Work orders, Defects, Inspection runs, Inspection templates, PM programs, Maintenance parts, History, Downtime, and reports when authorized.
- All work orders and defects for the tenant where the role allows manager access.
- Audit package exports when allowed by MaintainArr admin or manager access.

## What This Role Usually Cannot Access
- Does not own inventory balances or stock ledger; LoadArr owns inventory truth.
- Does not approve procurement as SupplyArr unless separately assigned.
- Does not own technician person records or training qualifications.

## Common Daily Tasks
- Review maintenance readiness.
- Create and assign work orders.
- Triage defects and open work orders from defects.
- Build inspection templates and PM programs.
- Request parts for work orders and review supply readiness.
- Close work orders after required evidence and review.

## Records This Role Works With
- asset
- defect
- work order
- inspection run
- inspection template
- PM program
- parts demand line
- downtime event

## Notifications This Role May Receive
- Assigned work and blockers
- defect escalation warnings
- PM due warnings
- parts readiness updates
- audit export completion

## Common Issues
- Work order is not assignable because technician or qualification data is missing.
- Parts are not available because LoadArr or SupplyArr data is missing.
- Asset readiness is blocked by defects, holds, or compliance gates.

## Related How-To Documents
- [How to create a work order](../how-to/maintainarr/how-to-create-a-work-order.md)
- [How to create a PM program](../how-to/maintainarr/how-to-create-a-pm-program.md)
- [How to close a work order](../how-to/maintainarr/how-to-close-a-work-order.md)
