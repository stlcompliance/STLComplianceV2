# How to confirm parts available for a work order

## Audience
Maintenance parts coordinators, warehouse users, inventory coordinators, and maintenance planners.

## Purpose
Check whether LoadArr can fulfill a MaintainArr work-order parts demand.

## Before You Start
- LoadArr access.
- The MaintainArr work order or parts demand reference.
- Part, item, lot, serial, location, and quantity requirements.

## Steps
1. Open LoadArr.
2. Open **Inventory**, **Reservations**, or the work-order parts fulfillment view when available.
3. Search for the part from the MaintainArr demand.
4. Review available quantity, reserved quantity, holds, location, lot, and serial requirements.
5. Reserve or issue the quantity if the workflow provides that action.
6. If stock is short, flag the shortage and coordinate with SupplyArr procurement.
7. Return to MaintainArr to confirm the work order reflects the fulfillment status.

## What Happens Next
LoadArr determines inventory availability and movement. MaintainArr keeps the work order status and installed parts context.

## Troubleshooting
- If availability and work order status disagree, check reservations, holds, and integration timing.
- If stock is short, use SupplyArr to procure instead of marking the work order fulfilled manually.
- If parts are held for quality review, check the AssurArr hold or LoadArr quarantine state before issuing.

## Related Docs
- [How to request parts for a work order](../maintainarr/how-to-request-parts-for-a-work-order.md)
- [How to handle a backordered part](../supplyarr/how-to-handle-a-backordered-part.md)

