# How to understand product access

## Audience
Users and admins checking why a user can or cannot open a product.

## Purpose
Separate sign-in, product entitlement, and product-specific permissions.

## Before You Start
- A signed-in account.
- Tenant context.
- Admin help if you need to change access.

## Steps
1. Confirm the user can sign in.
2. Check whether the product appears in the product switcher.
3. If the product appears, open it.
4. If the product opens but actions are missing, check product-specific role or permission assignment.
5. For StaffArr-managed permissions, review Roles, Permissions, and permission projection.

## What Happens Next
The user can launch products only when NexArr allows it and can act inside products only when product permissions allow it.

## Troubleshooting
- A visible product does not guarantee every action inside that product.
- A StaffArr role may be scoped to tenant, site, department, team, or position.
- Permission projection may need to refresh after role changes.

## Related Docs
- [Common permissions](../../reference/common-permissions.md)
- [Missing permission](../../troubleshooting/missing-permission.md)
