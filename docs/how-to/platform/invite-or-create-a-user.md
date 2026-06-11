# How to invite or create a user

## Audience
Platform admins and tenant admins

## Product
Platform Access

## Support Status
Supported by current UI/API

## Purpose
Create or invite a platform login while keeping the person record owned by StaffArr.

## Before You Start
- Create the StaffArr person record first when the user represents an internal person.
- You need platform user management access.
- Product permissions are assigned separately in StaffArr.

## Steps
1. Open Platform Admin.
2. Open Users.
3. Find the Create or invite user section.
4. Choose Invite when the person should set their password through the invitation flow, or Create when an admin is setting the initial account directly.
5. Enter the email and display name.
6. Set account flags such as active state, platform admin access, or email verification only when they are appropriate.
7. Submit the invite or creation action.
8. Add the user to the correct tenant membership.
9. Assign product permissions through StaffArr roles instead of treating the platform login as product authority.

## What Happens Next
NexArr owns the login, tenant membership, sessions, and platform roles. StaffArr remains the owner of the person and product authority context.

## Troubleshooting
- If the user can sign in but cannot open a product, verify tenant membership and product entitlement.
- If the user can open a product but cannot take actions, verify StaffArr role or permission assignments.

## Related How-To Documents
- [How to create a person](../staffarr/create-a-person.md)
- [How to assign a role](../staffarr/assign-a-role.md)

