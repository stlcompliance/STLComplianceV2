# How to review customer onboarding

## Audience
Onboarding reviewers, account managers, and compliance users.

## Purpose
Review a customer onboarding record and advance its canonical onboarding status.

## Before You Start
- CustomArr access.
- Permission to review onboarding.
- Required customer account, contact, location, eligibility, and requirement details.

## Steps
1. Open CustomArr.
2. Open Onboarding.
3. Select the customer onboarding record.
4. Review account details, contact authorization, locations, requirements, and communication preferences.
5. Record missing information or risk findings where the workflow supports it.
6. Approve, reject, pause, or return the onboarding record according to the available status actions.
7. Confirm the account summary reflects the canonical `CustomerOnboarding.status`.

## What Happens Next
`CustomerOnboarding.status` is the canonical onboarding signal. Customer account summaries may roll it up for display, but they do not replace it.

## Troubleshooting
- If the status action is missing, check `customarr.onboarding.review`.
- If service is restricted after approval, update service eligibility separately.
- If retained documents are required, store them through RecordArr.

## Related Docs
- [CustomArr guide](../../products/customarr-user-guide.md)
- [RecordArr guide](../../products/recordarr-user-guide.md)

## Availability
Supported by product contract/docs. UI labels may vary by deployment.
