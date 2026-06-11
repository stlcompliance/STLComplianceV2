# How to quarantine received items

## Audience
Warehouse receivers, quality users, and inventory supervisors

## Product
LoadArr

## Support Status
Supported by current UI/API

## Purpose
Place received inventory on hold so it cannot be used until reviewed or released.

## Before You Start
- LoadArr owns inventory holds and quarantine status.
- AssurArr may own a broader quality case if quality investigation is needed.
- SupplyArr owns vendor follow-up when the issue is supplier-related.

## Steps
1. Open LoadArr.
2. Open Receiving, Inventory, Holds, or Exceptions.
3. Select the item, receipt line, lot, serial, or balance.
4. Choose Create hold.
5. Select the hold type and quantity.
6. Enter the reason, source reference, and any notes required by the page.
7. Attach or reference evidence when needed.
8. Save the hold.
9. Confirm the quantity is not available for normal issue, transfer, or fulfillment.

## What Happens Next
LoadArr records the quarantine or hold and protects availability. Other products should consume this availability state instead of creating their own stock truth.

## Troubleshooting
- If the issue requires CAPA or nonconformance, open the AssurArr process when available.
- If vendor replacement is needed, coordinate through SupplyArr.

