# How to remove or deactivate access

## Audience
Platform admins, tenant admins, and StaffArr permission admins

## Product
Platform Access

## Support Status
Supported by current UI/API

## Purpose
Remove access without deleting product history or breaking source-of-truth ownership.

## Before You Start
- Confirm whether the person is leaving, changing jobs, or only losing one product permission.
- For employment or contractor status changes, start in StaffArr.

## Steps
1. For a person status change, open StaffArr and begin the offboarding or status update workflow.
2. Remove product role assignments in StaffArr when the person no longer has product authority.
3. Open Platform Admin Users if the platform login should be disabled, locked, or removed from a tenant.
4. Revoke active sessions when immediate access removal is required.
5. Revoke tenant entitlements only when the entire tenant should lose that product, not when one user should lose access.
6. Review the user audit and launch history if you need evidence of the access change.

## What Happens Next
The person record and personnel history remain in StaffArr. NexArr records platform access changes, tenant membership, and session revocation.

## Troubleshooting
- If a former user can still see a product, check both NexArr tenant membership and StaffArr role assignment.
- If a user needs temporary restriction rather than removal, use the StaffArr authority or role process instead of disabling the login.

## Related How-To Documents
- [How to deactivate or offboard a person](../staffarr/deactivate-or-offboard-a-person.md)

