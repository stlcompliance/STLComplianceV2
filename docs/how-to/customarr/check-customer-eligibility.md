# How to check customer eligibility

## Audience
Customer service users, order coordinators, dispatch coordinators, and managers

## Product
CustomArr

## Support Status
Supported by product contract/docs

## Purpose
Check whether a customer is eligible for service without confusing eligibility with account lifecycle.

## Before You Start
- Customer lifecycle status and service eligibility are separate.
- Contact authorization and customer requirements may affect whether a request can proceed.

## Steps
1. Open CustomArr.
2. Search for the customer account.
3. Review lifecycle status.
4. Review service eligibility, restrictions, access requirements, and customer requirements.
5. Confirm the requesting contact has the needed authorization.
6. Use the result in OrdArr or the execution product without editing execution truth from CustomArr.

## What Happens Next
OrdArr and execution products may consume eligibility context, but CustomArr remains the source of customer eligibility truth.

## Troubleshooting
- If eligibility blocks service, resolve requirements or restrictions before launching new work.
- If order readiness still fails after eligibility is clear, check OrdArr.

## Related How-To Documents
- [How to create an order request](../ordarr/create-an-order-request.md)
