# How to send or track a purchase order

## Audience
Supply chain users and purchasing coordinators

## Product
SupplyArr

## Support Status
Supported by current UI/API with intended external handoff

## Purpose
Send procurement context to the vendor and monitor whether the order is acknowledged, fulfilled, or blocked.

## Before You Start
- SupplyArr owns operational vendor order tracking.
- External finance or ERP systems own accounting execution.
- Vendor portal routes exist for vendor-facing order views.

## Steps
1. Open SupplyArr.
2. Open Purchasing.
3. Open Vendor orders.
4. Select the vendor order.
5. Review vendor, line items, destination, due date, and current status.
6. Use the available send, share, or portal action if present for the tenant.
7. Monitor acknowledgement, fulfillment, exception, and receiving-related status.
8. Use Notes or audit history to record operational follow-up when available.
9. Coordinate receiving in LoadArr when the goods arrive.

## What Happens Next
SupplyArr tracks vendor order state and LoadArr handles physical receiving. External finance systems handle bills and payments.

## Troubleshooting
- If no send action is visible, use the vendor order detail as the internal tracking record and confirm the configured vendor communication process.
- If goods arrived but the order still appears open, check LoadArr receiving status and SupplyArr status synchronization.

## Related How-To Documents
- [How to receive inbound goods](../loadarr/receive-inbound-goods.md)

