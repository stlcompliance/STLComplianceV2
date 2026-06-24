# How to handle customer context in SupplyArr

## Audience
Supply chain users who see customer context while working in SupplyArr.

## Purpose
Use customer context safely without creating or treating SupplyArr as the customer source of truth.

## Before You Start
- Permission to view the relevant SupplyArr procurement context.
- CustomArr owns customer accounts, contacts, locations, onboarding, requirements, and service eligibility.
- OrdArr owns order and request orchestration when customer work needs product handoffs.
- SupplyArr owns supplier/vendor, supplier contact, item, procurement, and purchase context.

## Steps
1. If you need to create or update a customer account, open CustomArr instead.
2. If you need to coordinate customer-requested work, open OrdArr instead.
3. In SupplyArr, use customer references only when the page clearly labels them as references, snapshots, or links.
4. Do not create a supplier, vendor, or party record just to stand in for a customer.
5. If a customer and supplier relationship both matter, keep the customer in CustomArr and the supplier in SupplyArr, then use the approved cross-product reference or order context.

## What Happens Next
SupplyArr can preserve procurement context without becoming the customer master. CustomArr remains the customer source of truth.

## Troubleshooting
- If a user asks you to create a customer in SupplyArr, send them to CustomArr customer creation.
- If a customer order needs supplier procurement, coordinate through OrdArr or the relevant product handoff instead of duplicating customer data.
- If a SupplyArr page shows a customer snapshot, treat it as read-only context unless the page explicitly says otherwise.

## Related Docs
- [SupplyArr guide](../../products/supplyarr-user-guide.md)
- [CustomArr guide](../../products/customarr-user-guide.md)
- [OrdArr guide](../../products/ordarr-user-guide.md)
- [How to create a customer](../customarr/how-to-create-a-customer.md)

## Ownership reminder
Customer creation belongs to CustomArr, not SupplyArr.
