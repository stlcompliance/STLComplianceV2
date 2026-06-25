# How to manage a user's product permissions

## Audience
Tenant administrators, StaffArr permission administrators, and authorized managers

## Product
StaffArr with NexArr-backed account context

## Purpose
Give a person the authority needed to work inside STL Compliance products. Every ordinary product is already available to active tenant members; this procedure manages actions, record scope, and delegated account status rather than product launch workflow state.

## Before you start
- Confirm the person exists in StaffArr.
- Confirm the platform account is linked to the correct person when login is required.
- Know the work, sites, departments, teams, and approval limits the person needs.
- Do not use platform-admin status as a substitute for product permissions.

## Steps
1. Open StaffArr **People** and select the person.
2. Open **Permissions** or **Assignments**.
3. Review effective permissions before adding another role.
4. Assign the smallest appropriate role template or explicit permission set.
5. Set tenant, site, department, team, position, or record scope as required.
6. Review cross-product effects and segregation-of-duties warnings.
7. Save and confirm the effective permission projection.
8. Have the user refresh the product if an already-open session has stale authority context.

## What happens next
The user can continue to launch every ordinary product. Each product exposes only pages and actions allowed by its server-side permission, scope, workflow-state, qualification, and blocker checks.

## Troubleshooting
- If the user cannot sign in or select the tenant, check NexArr account and tenant membership.
- If the product does not launch, check product operational status and launch/callback configuration.
- If the product opens but an action is unavailable, inspect effective StaffArr permissions, scope, workflow state, and product-owned rules.

## Related how-to documents
- [How to assign a role](../staffarr/assign-a-role.md)
- [How to edit role permissions](../staffarr/edit-role-permissions.md)
- [How to troubleshoot login or permission problems](troubleshoot-login-or-permission-problems.md)
