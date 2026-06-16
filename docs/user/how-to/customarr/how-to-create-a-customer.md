# How to create a customer

## Audience
Customer operations users, account managers, and onboarding reviewers.

## Purpose
Create a customer prospect or onboarding record in CustomArr, then activate the customer account when review is complete.

## Before You Start
- CustomArr access.
- Permission to create or manage customer onboarding.
- Legal name, display name, billing or service context, primary contact, and any known service eligibility constraints.

## Steps
1. Open CustomArr.
2. Open Customers, Prospects, or Onboarding.
3. Select the create action available for your role.
4. Enter the customer identity and account summary details.
5. Add the primary contact and initial location if the form supports it.
6. Save the prospect or onboarding record.
7. Complete required review steps before activating the customer account.
8. Set service eligibility separately from the customer account lifecycle status.

## What Happens Next
CustomArr owns the customer account and onboarding status. OrdArr may reference the customer for order requests, but OrdArr does not become the customer source of truth.

## Troubleshooting
- If the create action is missing, check `customarr.customers.manage` or `customarr.onboarding.review`.
- If the customer should not receive service yet, update service eligibility instead of changing the account lifecycle to an unrelated value.
- If the request started on the public site, confirm whether it was routed to NexArr tenant prospect intake, external CRM, email/manual review, or a future platform CRM.

## Related Docs
- [CustomArr guide](../../products/customarr-user-guide.md)
- [OrdArr guide](../../products/ordarr-user-guide.md)

## Availability
Supported by product contract/docs. UI labels may vary by deployment.
