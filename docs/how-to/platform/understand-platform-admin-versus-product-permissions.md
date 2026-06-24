# How to understand platform admin versus product permissions

## Audience
Administrators, product owners, and support users

## Purpose
Route an access or authority issue to the correct owner.

## Responsibility split
- **NexArr:** sign-in, accounts, tenant membership, sessions, platform-admin status, product registry/operational state, launch context, and service trust.
- **StaffArr:** person records, organizations/locations, role and permission assignments, scope, and delegated account-management workflows.
- **Owning product:** final server-side action authorization, workflow state, record scope, qualifications, holds, and domain blockers.
- **Compliance Core:** runtime rules/evaluations for all tenants; administrative studio for platform administrators only.

## Diagnostic order
1. Check NexArr for sign-in, membership, session, or launch failure.
2. Check StaffArr for effective role/permission and scope.
3. Check the owning product for workflow state and domain blockers.
4. Check the source owner when a referenced person, location, customer, item, asset, record, or other truth is missing.

Platform-admin status never substitutes for ordinary product permission and should not silently bypass tenant isolation or domain controls.
