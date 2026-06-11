# How to request parts for a work order

## Audience
Maintenance planners, technicians, and parts coordinators

## Product
MaintainArr

## Support Status
Supported by current UI/API

## Purpose
Request parts needed for maintenance without making MaintainArr the inventory owner.

## Before You Start
- MaintainArr may request parts.
- LoadArr owns inventory availability, reservations, issues, and stock movement.
- SupplyArr owns item, part, vendor, and procurement context.

## Steps
1. Open MaintainArr.
2. Open Work orders and select the work order.
3. Open the parts or materials demand area.
4. Add the needed part, quantity, need date, and reason when the form asks for them.
5. Submit the parts demand.
6. Review availability, reservation, or fulfillment status returned by LoadArr/SupplyArr.
7. If stock is unavailable, coordinate procurement through SupplyArr.
8. Record installed parts on the work order when the maintenance work is completed.

## What Happens Next
MaintainArr keeps the work demand and usage context. LoadArr keeps stock movement truth, and SupplyArr handles procurement context when stock is short.

## Troubleshooting
- If a part cannot be found, check SupplyArr catalog/part master.
- If quantity looks wrong, check LoadArr availability and reservations instead of editing the work order demand as inventory truth.

## Related How-To Documents
- [How to confirm parts available for a work order](../loadarr/confirm-parts-available-for-a-work-order.md)
- [How to handle a backordered part](../supplyarr/handle-a-backordered-part.md)

