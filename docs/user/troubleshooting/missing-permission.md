# I am missing a permission

## Symptoms
- A button is hidden or disabled.
- A page opens read-only.
- An action fails with a forbidden message.

## Likely Causes
- Product entitlement exists but product permission is missing.
- Role assignment scope is wrong.
- Permission projection has not refreshed.
- Record status does not allow the action.

## What to Check
1. Confirm the product is visible.
2. Check the exact action you cannot perform.
3. Ask a StaffArr admin to review role assignments and permission projection.
4. Check the record status.

## How to Fix
- Assign the correct role or permission.
- Fix role scope.
- Refresh or process permission projection.
- Move the record through the required workflow state.

## Who Can Help
StaffArr admin, product admin, or tenant admin.

## Related Docs
- [Common permissions](../reference/common-permissions.md)
- [Profile and access](../getting-started/profile-and-access.md)
