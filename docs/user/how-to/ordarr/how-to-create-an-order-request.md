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
2. Open Orders, Requests, or Intake.
3. Select the create action available for your role.
4. Add the customer reference, request details, requested dates, participants, locations, and lines.
5. Review readiness checks and missing information.
6. Save the request.
7. Move the request to triage when enough information is available.

## What Happens Next
OrdArr owns the order or request lifecycle. CustomArr remains the customer source of truth, and execution products own the work they perform.

## Troubleshooting
- If the customer is missing, create or resolve the customer in CustomArr.
- If the create action is missing, check `ordarr.orders.create`.
- If a requested product is not available, check NexArr entitlement and OrdArr handoff configuration.

## Related Docs
- [OrdArr guide](../../products/ordarr-user-guide.md)
- [CustomArr guide](../../products/customarr-user-guide.md)

## Availability
Supported by product contract/docs. UI labels may vary by deployment.
