# How to assign a role

## Audience
StaffArr permission admins and tenant admins

## Product
StaffArr

## Support Status
Supported by current UI/API

## Purpose
Give a person product authority through StaffArr role assignment.

## Before You Start
- The person needs an active tenant membership; ordinary products do not require a separate product-availability approval.
- You need StaffArr role assignment access.

## Steps
1. Open StaffArr.
2. Open Roles.
3. Select the role to assign.
4. Open the Assignments tab.
5. Choose Assign role.
6. Select the person or scope requested by the assignment form.
7. Review product, module, and scope access before saving.
8. Save the assignment.
9. Ask the user to refresh the product if an action remains disabled.

## What Happens Next
StaffArr records the permission assignment and consuming products apply product-local authorization from that role context.

## Troubleshooting
- If the user cannot open the product, fix NexArr account or tenant membership first.
- If the user can open the product but cannot act, verify the role includes the needed module and scope.

## Related How-To Documents
- [How to manage a user's product permissions](../platform/manage-a-users-product-permissions.md)
- [How to edit role permissions](../staffarr/edit-role-permissions.md)
