# How to add a customer location

## Audience
Customer operations users, account managers, dispatch coordinators, and compliance users

## Product
CustomArr

## Support Status
Supported by product contract/docs

## Purpose
Add a customer-owned service, pickup, delivery, or billing location without duplicating customer location truth in StaffArr or execution products.

## Before You Start
- CustomArr owns customer locations.
- StaffArr owns internal tenant sites and locations.
- RoutArr, LoadArr, and other products may reference or snapshot customer locations for execution.

## Steps
1. Open CustomArr.
2. Open the customer account.
3. Open Locations.
4. Add location identity, address or operating description, and contact instructions.
5. Record access instructions and customer access requirements.
6. Link enforceable requirements to customer requirements when needed.
7. Save the location.

## What Happens Next
Execution products can reference the CustomArr location. They should not become the canonical customer location owner.

## Troubleshooting
- If a stop needs a customer location, reference CustomArr from RoutArr rather than creating a duplicate canonical location.
- If a location blocks service, review eligibility and requirements separately from lifecycle status.

## Related How-To Documents
- [How to check customer eligibility](check-customer-eligibility.md)
