# How to receive inbound goods

## Audience
Warehouse receivers and inventory coordinators

## Product
LoadArr

## Support Status
Supported by current UI/API

## Purpose
Record physical receipt of inbound goods and start inventory control in LoadArr.

## Before You Start
- LoadArr owns receiving, expected receipts, inventory balances, stock ledger, and warehouse movement.
- SupplyArr owns vendor/order context.
- StaffArr owns internal location references.

## Steps
1. Open LoadArr.
2. Open Expected Receipts or Receiving.
3. Select the inbound receipt or create the receiving work from the available queue.
4. Confirm vendor, item, quantity expected, destination, lot, serial, and receiving location details.
5. Enter the received quantity.
6. Record exceptions, overages, shortages, damage, lot, serial, or hold information when applicable.
7. Choose Complete receiving when the receipt is ready.
8. Send items to staging, putaway, quarantine, or exception handling as directed by the workflow.

## What Happens Next
LoadArr records actual receipt and updates receiving history, stock ledger, and availability according to the workflow outcome.

## Troubleshooting
- If the expected receipt is missing, check SupplyArr vendor order or procurement handoff.
- If goods are damaged or mismatched, complete the receiving exception path instead of forcing a clean receipt.

