# A work order is not assignable

## Symptoms
- Technician picker is missing a person.
- Assignment action fails.
- Work order shows a qualification blocker.

## Likely Causes
- Technician person reference is missing.
- Technician lacks required TrainArr qualification.
- User lacks work order manage access.
- Work order status does not allow assignment.

## What to Check
1. Confirm technician exists in StaffArr.
2. Check TrainArr qualification status.
3. Check MaintainArr permissions.
4. Review work order status and blockers.

## How to Fix
- Create or sync technician reference.
- Assign or complete required training.
- Use an authorized manager to assign.
- Resolve blockers before assignment.

## Who Can Help
MaintainArr manager, StaffArr admin, or trainer.

## Related Docs
- [How to create a work order](../how-to/maintainarr/how-to-create-a-work-order.md)
