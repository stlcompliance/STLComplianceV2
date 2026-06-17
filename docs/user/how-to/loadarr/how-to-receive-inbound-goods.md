# How to receive inbound goods

## Audience
Warehouse receivers and LoadArr users with receiving access.

## Purpose
Start or complete a LoadArr receiving session for inbound goods.

## Before You Start
- LoadArr access.
- Receiving create or confirm permission as needed.
- Expected receipt, purchase order receipt, or shipment information.
- Receiving location.

![LoadArr receiving session showing received item quantities, item condition, and exception controls.](/screenshots/loadarr-receiving.png "Confirm received items and create an exception when quantity or condition does not match.")

## Steps
1. Open LoadArr.
2. Open **Work** > **Receiving**.
3. Select an existing receiving session or select the create receiving action if available.
4. Choose the receiving type and source record if shown.
5. Enter or confirm received items and quantities.
6. Record condition such as New, Pending inspection, Damaged, or Quarantined where shown.
7. Create a receiving exception if quantities or condition do not match.
8. Confirm the receiving session when complete.

## What Happens Next
LoadArr records the receiving outcome and stock movement. Items may move to putaway, staging, hold, quarantine, or exception review depending on status.

## Troubleshooting
- If the expected receipt is missing, check **Expected Receipts** or SupplyArr purchase order context.
- If quantities do not match, create a receiving exception instead of forcing a clean receipt.
- If the item needs quality review, use quarantine or exception handling.

## Related Docs
- [Receiving exception](../../troubleshooting/receiving-exception.md)
- [PO to putaway workflow](../../workflows/po-to-putaway.md)
