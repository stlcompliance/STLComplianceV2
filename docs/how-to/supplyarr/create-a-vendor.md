# How to create a vendor

## Audience
Supply chain users and SupplyArr admins

## Product
SupplyArr

## Support Status
Supported by current UI/API

## Purpose
Create the vendor or supplier record used for procurement, vendor orders, pricing, lead time, and supply readiness.

## Before You Start
- SupplyArr owns vendor and supplier master records.
- QuickBooks or ERP owns financial execution; SupplyArr may store operational mappings or snapshots.

## Steps
1. Open SupplyArr.
2. Open Parties.
3. Choose Create.
4. Create the party as a vendor or supplier using the fields available on the page.
5. Add contacts, requirements, documents, external IDs, and operational notes when the form supports them.
6. Save the vendor.
7. Review the party detail for status, procurement context, pricing, lead time, and audit history.
8. Use RecordArr for long-term vendor documents when documents need retention or controlled access.

## What Happens Next
The vendor becomes the SupplyArr-owned source for procurement and supplier context. Financial payment and accounting remain external.

## Troubleshooting
- If the vendor must be paid through QuickBooks or ERP, confirm external mapping rather than treating SupplyArr as accounts payable.
- If the party is a customer, create or update it in CustomArr. Use SupplyArr only for supplier/vendor records or labeled customer references.

## Related How-To Documents
- [How to create a customer](../customarr/create-a-customer.md)
- [How to handle customer context in SupplyArr](../supplyarr/create-a-customer.md)
