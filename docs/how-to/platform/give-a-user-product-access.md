# How to give a user product access

## Audience
Platform admins, tenant admins, and StaffArr permission admins

## Product
Platform Access

## Support Status
Supported by current UI/API

## Purpose
Grant product access in the correct order: tenant entitlement, user membership, then product-domain permissions.

## Before You Start
- You need access to Platform Admin and StaffArr role management.
- Do not use platform admin status as a substitute for product permissions.

## Steps
1. Open Platform Admin and confirm the tenant has the needed product entitlement.
2. If the tenant is missing the product, grant the entitlement from the tenant detail view.
3. Open Users and confirm the user belongs to the tenant.
4. Add the tenant membership if it is missing.
5. Open StaffArr Roles.
6. Assign the product role or permission scope that matches the user work.
7. Ask the user to refresh the suite or sign out and back in if the product does not appear immediately.

## What Happens Next
NexArr controls launch eligibility. StaffArr controls product authority and role assignments after launch.

## Troubleshooting
- If the product still does not appear, check entitlement, membership, and product switcher visibility.
- If actions are disabled inside the product, check the StaffArr role assignments and product-local permission checks.

## Related How-To Documents
- [How to assign a role](../staffarr/assign-a-role.md)
- [How to edit role permissions](../staffarr/edit-role-permissions.md)

