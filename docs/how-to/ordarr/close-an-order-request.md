# How to close an order request

## Audience
Order coordinators, operations managers, and customer service users

## Product
OrdArr

## Support Status
Supported by product contract/docs

## Purpose
Close an order request after required execution work, evidence, and completion checks are ready.

## Before You Start
- Execution handoffs have been reviewed.
- Required completion evidence is available through execution products or RecordArr.

## Steps
1. Open OrdArr.
2. Open the order request.
3. Review order lines, handoff statuses, exceptions, and required completion evidence.
4. Resolve open coordination exceptions.
5. Build or refresh the completion packet if available.
6. Close the order request using the available status action.
7. Confirm invoice-ready or bill-ready packet state if finance handoff is needed.

## What Happens Next
OrdArr owns the closed order state and completion packet. External finance systems own final accounting.

## Troubleshooting
- If close is blocked, check open handoffs, unresolved exceptions, missing evidence, and packet readiness.
- If files are missing, check RecordArr.

## Related How-To Documents
- [How to prepare financial handoff packets](prepare-financial-handoff-packets.md)
