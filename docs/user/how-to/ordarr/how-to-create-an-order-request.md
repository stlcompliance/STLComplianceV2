# How to create an order request

## Audience
Customer service users, order coordinators, and operations coordinators.

## Purpose
Create the OrdArr parent request that coordinates customer work across products.

## Before You Start
- OrdArr access.
- Permission to create orders or requests.
- Customer reference from CustomArr when the work is customer-facing.
- Known requested service, requested timing, location, contact, and required execution products.

## Steps
1. Open OrdArr.
2. Open Orders.
3. Select `Create order`.
4. Add the customer reference, request summary, source channel, priority, requested windows, and an initial line.
5. Save the order to create the draft record.
6. Open the order detail page to add more lines, place holds, approve, or submit when ready.

## What Happens Next
OrdArr owns the order or request lifecycle. CustomArr remains the customer source of truth, and execution products own the work they perform.

## Troubleshooting
- If the customer is missing, create or resolve the customer in CustomArr.
- If the create action is missing, check `ordarr.orders.create`.
- If a requested product is not available, check destination operational status and OrdArr handoff configuration.

## Related Docs
- [OrdArr guide](../../products/ordarr-user-guide.md)
- [CustomArr guide](../../products/customarr-user-guide.md)

## Availability
Supported by product contract/docs. UI labels may vary by deployment.
