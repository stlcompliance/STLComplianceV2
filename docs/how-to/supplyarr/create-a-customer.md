# How to create a customer

## Audience
Sales, operations, and supply chain users

## Product
SupplyArr

## Support Status
Placeholder

## Current State
- The ownership constitution assigns customer master records to CustomArr.
- SupplyArr owns vendors, suppliers, items, procurement context, and operational vendor/customer references only where explicitly modeled.
- The current requested workflow conflicts with the ownership rule if SupplyArr is treated as the canonical customer source of truth.

## Expected Direction
- Customer creation should live in CustomArr when that product is present.
- SupplyArr may reference a customer or external party only as a labeled reference or snapshot if the applicable product docs allow it.
- A future how-to should point to the CustomArr customer creation workflow or a clearly labeled cross-product reference workflow.

## Open Questions
- Is CustomArr enabled in this deployment?
- Should SupplyArr Parties allow non-canonical customer references, or should users be routed away from SupplyArr?
- Which UI label should distinguish vendor/supplier records from customer references?

## Related How-To Documents
- [How to create a vendor](../supplyarr/create-a-vendor.md)

