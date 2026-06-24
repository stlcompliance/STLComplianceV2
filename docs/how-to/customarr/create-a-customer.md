# How to create a customer

## Audience
Customer operations users, account managers, and onboarding reviewers

## Product
CustomArr

## Support Status
Supported by product contract/docs

## Purpose
Create a customer prospect or onboarding record in CustomArr, then activate the customer account after review.

## Before You Start
- CustomArr owns customer account, onboarding, contact, location, requirement, and service eligibility truth.
- Public-site leads hand off to CustomArr through the approved idempotent intake contract; CustomArr owns accepted lead and relationship truth.
- OrdArr owns customer order and request orchestration after a customer can be referenced.

## Steps
1. Open CustomArr.
2. Open Customers, Prospects, or Onboarding.
3. Choose Create.
4. Enter customer identity, account summary, primary contact, and initial service context.
5. Save the prospect or onboarding record.
6. Complete required review steps.
7. Activate the customer account when onboarding is approved.
8. Set service eligibility separately from account lifecycle status.

## What Happens Next
CustomArr becomes the customer source of truth. OrdArr and execution products may reference the customer but do not replace it.

## Troubleshooting
- If the customer cannot be served yet, update service eligibility rather than changing lifecycle to an unrelated status.
- If the request came from the public site, check the CustomArr lead intake/review queue and any configured external-CRM connector.
- If documents need retention, store them through RecordArr.

## Related How-To Documents
- [How to review customer onboarding](review-customer-onboarding.md)
- [How to create an order request](../ordarr/create-an-order-request.md)
