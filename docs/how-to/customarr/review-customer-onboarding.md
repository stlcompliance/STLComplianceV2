# How to review customer onboarding

## Audience
Onboarding reviewers, account managers, and compliance users

## Product
CustomArr

## Support Status
Supported by product contract/docs

## Purpose
Review the canonical `CustomerOnboarding.status` and decide whether the customer can move forward.

## Before You Start
- Customer onboarding status belongs to CustomArr.
- Account summaries may roll up onboarding status for display but are not the canonical onboarding record.

## Steps
1. Open CustomArr.
2. Open Onboarding.
3. Select the customer onboarding record.
4. Review account details, contact authorization, locations, requirements, preferences, and risk notes.
5. Record missing information or findings.
6. Approve, reject, pause, or return the onboarding record using the available status action.
7. Confirm the account summary reflects the onboarding result.

## What Happens Next
The customer account can become active when onboarding is approved, but service eligibility remains a separate signal.

## Troubleshooting
- If review actions are hidden, check `customarr.onboarding.review`.
- If required evidence is missing, use RecordArr for retained files.

## Related How-To Documents
- [How to create a customer](create-a-customer.md)
- [How to check customer eligibility](check-customer-eligibility.md)
