# How to confirm parts available for a work order

## Audience
Maintenance parts coordinators and warehouse users

## Product
LoadArr

## Support Status
Supported by current UI/API with cross-product demand

## Purpose
Check whether LoadArr can fulfill a MaintainArr work-order parts demand.

## Before You Start
- MaintainArr owns the work order and parts demand.
- LoadArr owns inventory balances, reservations, holds, and issues.
- SupplyArr owns part/item and procurement context.

## Steps
1. Open LoadArr.
2. Open Inventory, Reservations, or the work-order parts fulfillment view when available.
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

## Related How-To Documents
- [How to request parts for a work order](../maintainarr/request-parts-for-a-work-order.md)
- [How to handle a backordered part](../supplyarr/handle-a-backordered-part.md)

