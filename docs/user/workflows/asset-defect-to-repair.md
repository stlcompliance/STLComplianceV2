# Asset Defect to Repair

## Purpose
Move from a reported asset defect to maintenance repair and readiness update.

## Who Participates
- Operator or technician
- Maintenance manager
- Technician

## Starting Point
A defect is found on an asset.

## Main Steps
1. Report the defect in MaintainArr.
2. Manager reviews severity and status.
3. Open a work order from the defect when needed.
4. Assign technician and add tasks.
5. Complete work, labor, and evidence.
6. Close the work order after review.
7. Review asset readiness.

## Products Involved
- MaintainArr owns asset, defect, work order, and readiness.
- TrainArr may provide technician qualification checks.
- LoadArr and SupplyArr may support parts.
- RecordArr may store evidence.

## Records Created or Updated
- defect
- work order
- labor entry
- evidence
- readiness history

## Where Users May Get Stuck
- Asset not found.
- Technician not assignable.
- Parts not available.
- Work order cannot close because evidence or blockers remain.

## Related How-To Docs
- [How to create a defect report](../how-to/maintainarr/how-to-create-a-defect-report.md)
- [How to close a work order](../how-to/maintainarr/how-to-close-a-work-order.md)
