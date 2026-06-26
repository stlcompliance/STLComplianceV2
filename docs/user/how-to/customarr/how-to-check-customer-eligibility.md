# How to check customer eligibility

## Audience
Customer service users, order coordinators, dispatch coordinators, and managers.

## Purpose
Check whether a customer is eligible for service without confusing eligibility with customer account lifecycle.

## Before You Start
- CustomArr access.
- Customer account or order context.
- Permission to view customer account and service profile details.

## Steps
1. Open CustomArr.
2. Search for the customer account.
3. Review account lifecycle status.
4. Review service eligibility status and any restrictions, access requirements, or customer requirements.
5. Check contact authorization if a customer contact is requesting action.
6. Use the result in OrdArr or the execution product without editing execution truth from CustomArr.

## What Happens Next
Eligibility can block or shape service while the account lifecycle remains separate. OrdArr and execution products consume the eligibility context for their own decisions.

## Troubleshooting
- If the customer exists but is not service eligible, review requirements and restrictions before creating new work.
- If an order cannot proceed, check OrdArr readiness and execution handoffs after confirming CustomArr eligibility.
- If the issue is a login or portal-access problem, check NexArr and the related CustomArr portal access record.

## Related Docs
- [CustomArr guide](../../products/customarr-user-guide.md)
- [OrdArr guide](../../products/ordarr-user-guide.md)

## Availability
Supported by product contract/docs. UI labels may vary by deployment.
