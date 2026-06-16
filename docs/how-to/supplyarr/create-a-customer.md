# How to handle customer context in SupplyArr

## Audience
Sales, operations, and supply chain users

## Product
SupplyArr

## Support Status
Boundary guidance

## Current State
- CustomArr owns customer accounts, contacts, locations, onboarding, requirements, and service eligibility.
- OrdArr owns order and request orchestration for customer-facing work.
- SupplyArr owns vendors, suppliers, supplier contacts, tenant commercial items/parts/materials/SKUs, procurement context, and supplier locations.

## Purpose
Use customer context safely in SupplyArr without creating a duplicate customer master record.

## Steps
1. If you need to create or update a customer account, use CustomArr.
2. If you need to coordinate customer-requested work across products, use OrdArr.
3. Use customer references in SupplyArr only when the field is explicitly labeled as a reference, snapshot, or link.
4. Keep supplier/vendor records in SupplyArr and customer records in CustomArr.
5. Do not create a SupplyArr party as a workaround for missing customer data.

## What Happens Next
SupplyArr can show procurement context tied to a customer-facing request, but CustomArr remains the customer source of truth.

## Troubleshooting
- If a buyer needs customer-specific procurement context, start from OrdArr or the approved cross-product reference.
- If the customer account is wrong, fix it in CustomArr.
- If the supplier is wrong, fix it in SupplyArr.

## Related How-To Documents
- [How to create a vendor](../supplyarr/create-a-vendor.md)
- [How to create a customer](../customarr/create-a-customer.md)
- [How to create an order request](../ordarr/create-an-order-request.md)
