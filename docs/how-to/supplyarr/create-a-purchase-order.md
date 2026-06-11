# How to create a purchase order

## Audience
Supply chain users and purchasing approvers

## Product
SupplyArr

## Support Status
Supported by current UI/API with vendor-order surface

## Purpose
Create operational procurement intent or a vendor order for needed goods or services.

## Before You Start
- SupplyArr owns purchase intent, procurement status, and operational PO metadata.
- QuickBooks or ERP owns financial bills, payments, tax, and general ledger.
- The UI exposes Purchasing and Vendor orders; exact purchase order labels may vary by tenant.

## Steps
1. Open SupplyArr.
2. Open Purchasing.
3. Review procurement demand, approvals, exceptions, or vendor orders.
4. Choose the purchase request, purchase order, or vendor order create action available in the workspace.
5. Select the vendor and item or part lines.
6. Enter quantities, need dates, destination, and operational notes.
7. Review approvals or exceptions before submitting.
8. Create or submit the order.
9. Track vendor order status from Purchasing or Vendor orders.

## What Happens Next
SupplyArr records procurement status and can publish expected receiving context to LoadArr. Financial execution remains in the external finance system.

## Troubleshooting
- If the action is labeled Vendor order rather than Purchase order, use the vendor order workflow and keep the document language operational.
- If stock is urgently needed, use the emergency PR path only if the tenant has enabled it.

## Related How-To Documents
- [How to receive inbound goods](../loadarr/receive-inbound-goods.md)

