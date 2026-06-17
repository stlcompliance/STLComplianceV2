# How to connect procurement expectations to receiving

## Audience
Supply chain users, buyers, warehouse coordinators, and receiving supervisors.

## Purpose
Make sure ordered goods become expected receipts for LoadArr without changing inventory from SupplyArr.

## Before You Start
- SupplyArr access.
- LoadArr receiving visibility if you need to confirm the handoff.
- Vendor order, item lines, quantities, destination, due date, and receiving location reference.

## Steps
1. Open SupplyArr.
2. Open **Purchasing** or **Vendor orders**.
3. Select the procurement record that will result in inbound goods.
4. Confirm vendor, item lines, quantities, destination, due date, and receiving location reference.
5. Use the available expected receipt, publish, or receiving handoff action when present.
6. Open LoadArr **Expected receipts** or **Receiving** to confirm the inbound work appears.
7. Update SupplyArr if vendor timing changes.
8. Complete physical receipt in LoadArr when goods arrive.

## What Happens Next
SupplyArr provides the procurement expectation. LoadArr records actual receipt and inventory movement.

## Troubleshooting
- If the expected receipt does not appear in LoadArr, check whether the handoff action exists for the tenant and whether the destination location is a StaffArr location.
- Do not manually increase LoadArr stock from SupplyArr.
- If timing changed, update the vendor order before warehouse users plan receiving work.

## Related Docs
- [How to create a purchase order](how-to-create-a-purchase-order.md)
- [How to receive inbound goods](../loadarr/how-to-receive-inbound-goods.md)

