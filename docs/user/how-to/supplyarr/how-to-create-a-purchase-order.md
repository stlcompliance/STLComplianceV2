# How to create a purchase order

## Audience
SupplyArr buyers, managers, and procurement users.

## Purpose
Create procurement intent or a vendor order from approved purchasing work.

## Before You Start
- SupplyArr access.
- Purchase request create access for draft work.
- Purchase request approval access if approval is required.
- Vendor and part records.

## Steps
1. Open SupplyArr.
2. Open **Purchasing**.
3. Create a purchase request draft with **Create draft** when the request does not exist yet.
4. Select the draft and choose **Submit for approval**.
5. An approver selects **Approve** or **Reject**.
6. Open **Vendor orders** or **Create vendor order** when the approved request should become a vendor order.
7. Enter required vendor order details.
8. Save or send the vendor order using the visible action.

## What Happens Next
SupplyArr records procurement status and vendor order context. Financial bills and payments remain outside STL Compliance.

## Troubleshooting
- If **Create draft** is missing, check supplyarr.purchaseRequests.create access.
- If **Approve** is missing, check supplyarr.purchaseRequests.approve access.
- If receiving is needed, LoadArr owns physical receiving.

## Related Docs
- [PO to putaway workflow](../../workflows/po-to-putaway.md)
- [How to receive inbound goods](../loadarr/how-to-receive-inbound-goods.md)
