# How to add a customer location

## Audience
Customer operations users, account managers, dispatch coordinators, and compliance users.

## Purpose
Add a customer-owned service, pickup, delivery, or billing location without making StaffArr or RoutArr the customer location owner.

## Before You Start
- CustomArr access.
- Customer account selected.
- Location name, address or operational description, contact instructions, and access requirements.

## Steps
1. Open CustomArr.
2. Open the customer account.
3. Open Locations.
4. Add the customer location details.
5. Record access instructions and customer access requirements.
6. If an access requirement is enforceable, link or compile it to the relevant Customer Requirement rather than leaving it as a local instruction only.
7. Save the location.

## What Happens Next
CustomArr owns customer location truth. RoutArr, LoadArr, and other execution products may reference or snapshot the location for execution work.

## Troubleshooting
- If the location is an internal facility, use StaffArr site/location records instead.
- If a route stop needs this location, confirm the route references the CustomArr location instead of creating a duplicate address as canonical.
- If the location blocks service, update service eligibility or requirements separately from lifecycle status.

## Related Docs
- [CustomArr guide](../../products/customarr-user-guide.md)
- [RoutArr guide](../../products/routarr-user-guide.md)

## Availability
Supported by product contract/docs. UI labels may vary by deployment.
