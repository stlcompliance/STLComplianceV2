# How to create a work order

## Audience
Maintenance planners, technicians, and supervisors

## Product
MaintainArr

## Support Status
Supported by current UI/API

## Purpose
Create maintenance work for an asset, defect, inspection result, or operational request.

## Before You Start
- The asset should already exist in MaintainArr.
- Use LoadArr for inventory availability and SupplyArr for purchasable part context.

## Steps
1. Open MaintainArr.
2. Open Work orders.
3. Choose Create.
4. Select the asset and work type.
5. Enter the problem, requested work, priority, due date, and assigned team or person when those fields are available.
6. Add parts demand or labor expectations if known.
7. Attach or reference related defects, inspections, or records when available.
8. Save the work order.
9. Open the work order detail to plan parts, labor, evidence, and closeout.

## What Happens Next
MaintainArr owns the work order lifecycle. Parts availability and stock movement remain owned by LoadArr.

## Troubleshooting
- If the asset is missing, create the asset first.
- If required parts are unavailable, use the parts demand path to SupplyArr and LoadArr rather than manually changing stock.

## Related How-To Documents
- [How to request parts for a work order](../maintainarr/request-parts-for-a-work-order.md)

