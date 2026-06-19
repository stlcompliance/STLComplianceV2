# How to close an order request

## Audience
Order coordinators, operations managers, and customer service users.

## Purpose
Close an order request after required execution work, evidence, and completion checks are ready.

## Before You Start
- OrdArr access.
- Order request selected.
- Execution handoffs reviewed.
- Required completion evidence available through execution products or RecordArr.

## Steps
1. Open OrdArr.
2. Open the order request.
3. Review order lines, handoff statuses, exceptions, and required completion evidence.
4. Resolve open holds, coordination exceptions, and missing evidence.
5. Build or refresh the completion packet if the workflow supports it.
6. Review any return or RMA records that affect closeout.
7. Close the order request using the available status action.
8. Confirm any invoice-ready or bill-ready packet state needed for finance handoff.

## What Happens Next
OrdArr owns the closed order state and completion packet. Execution products keep their own execution records, and external finance systems own final accounting.

## Troubleshooting
- If close is blocked, check open handoffs, unresolved exceptions, missing required evidence, and packet readiness.
- If retained files are missing, check RecordArr.
- If billing details are incomplete, review the invoice-ready or bill-ready packet before exporting.

## Related Docs
- [OrdArr guide](../../products/ordarr-user-guide.md)
- [RecordArr guide](../../products/recordarr-user-guide.md)

## Availability
Supported by product contract/docs. UI labels may vary by deployment.
