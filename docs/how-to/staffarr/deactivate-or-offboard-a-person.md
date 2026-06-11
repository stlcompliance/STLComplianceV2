# How to deactivate or offboard a person

## Audience
HR, operations managers, and StaffArr admins

## Product
StaffArr

## Support Status
Supported by current UI/API

## Purpose
End or restrict a person record while preserving personnel history and cross-product audit context.

## Before You Start
- StaffArr owns person status and personnel history.
- Use NexArr only for platform login/session actions.

## Steps
1. Open StaffArr.
2. Open People and find the person.
3. Open the person detail or offboarding area.
4. Start the offboarding or status update workflow.
5. Enter the effective date, reason, and any notes required by the form.
6. Review active roles, teams, assignments, and restrictions before confirming.
7. Save or execute the offboarding action.
8. If immediate login removal is needed, disable or lock the NexArr user and revoke sessions from Platform Admin.

## What Happens Next
StaffArr keeps the person history while other products consume the updated person or readiness state.

## Troubleshooting
- If the person still appears assignable in a product, verify that product has refreshed its StaffArr person and readiness reference.
- If only platform access should be removed, do not change the StaffArr person status unnecessarily.

## Related How-To Documents
- [How to remove or deactivate access](../platform/remove-or-deactivate-access.md)

