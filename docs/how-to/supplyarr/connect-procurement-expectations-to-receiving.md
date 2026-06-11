# How to connect procurement expectations to receiving

## Audience
Supply chain users and warehouse coordinators

## Product
SupplyArr

## Support Status
Supported by current UI/API with intended handoff flow

## Purpose
Make sure ordered goods become expected receipts for LoadArr without changing inventory from SupplyArr.

## Before You Start
- SupplyArr owns vendor/order/procurement context.
- LoadArr owns expected receipts, receiving workflow, inventory balances, and stock ledger.

## Steps
1. Open SupplyArr.
2. Open Purchasing or Vendor orders.
3. Select the procurement record that will result in inbound goods.
4. Confirm vendor, item lines, quantities, destination, due date, and receiving location reference.
5. Use the available expected receipt, publish, or receiving handoff action when present.
6. Open LoadArr Expected Receipts or Receiving to confirm the inbound work appears.
7. Update SupplyArr if vendor timing changes.
8. Complete physical receipt in LoadArr when goods arrive.

## What Happens Next
SupplyArr provides the procurement expectation. LoadArr records actual receipt and inventory movement.

## Troubleshooting
- If the expected receipt does not appear in LoadArr, check whether the handoff action exists for the tenant and whether the destination location is a StaffArr location.
- Do not manually increase LoadArr stock from SupplyArr.

## Related How-To Documents
- [How to receive inbound goods](../loadarr/receive-inbound-goods.md)

