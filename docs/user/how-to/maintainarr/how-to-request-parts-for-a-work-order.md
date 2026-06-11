# How to request parts for a work order

## Audience
Maintenance managers and technicians allowed to update work orders.

## Purpose
Add parts demand to a work order so SupplyArr and LoadArr can support fulfillment.

## Before You Start
- MaintainArr access.
- A selected work order.
- Part number, SupplyArr part reference, quantity, unit of measure, and notes if needed.

## Steps
1. Open MaintainArr.
2. Open **Work orders** and select the work order.
3. Find the parts demand or supply readiness area.
4. Enter part number or SupplyArr part id.
5. Enter quantity and unit of measure.
6. Add notes if useful.
7. Choose whether to create a purchase request draft if the option is shown.
8. Select the action to add the parts demand line.
9. Publish the parts demand when ready.

## What Happens Next
MaintainArr records the demand. SupplyArr and LoadArr may use the demand to evaluate procurement and inventory fulfillment.

## Troubleshooting
- If the part is unknown, check SupplyArr part catalog or MaintainArr maintenance part profiles.
- If parts are not available, check LoadArr inventory and SupplyArr procurement status.
- If publishing fails, confirm required part details are present.

## Related Docs
- [Part request to receiving workflow](../../workflows/part-request-to-receiving.md)
- [Parts not available](../../troubleshooting/parts-not-available.md)
