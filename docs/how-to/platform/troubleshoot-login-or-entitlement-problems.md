# How to troubleshoot login or entitlement problems

## Audience
Platform admins and support users

## Product
Platform Access

## Support Status
Supported by current UI/API

## Purpose
Find whether an access problem is caused by login state, tenant membership, product entitlement, or product launch configuration.

## Before You Start
- You need platform admin access.
- Have the user email, tenant, and product name ready.

## Steps
1. Open Platform Admin Users and search for the user.
2. Confirm the account is active, not locked, and associated with the expected tenant.
3. Review login, launch, and session history for recent failures.
4. Open Platform Admin Tenants and verify the tenant entitlement for the product.
5. Check product launch or callback settings if the handoff fails after entitlement is valid.
6. Use the platform status and audit views for broader service or configuration issues.
7. After fixing the issue, ask the user to retry from the suite sign-in page or product switcher.

## What Happens Next
Successful launch means NexArr allowed identity, tenant, entitlement, and product handoff. Product actions may still require StaffArr or product permissions.

## Troubleshooting
- If login succeeds but product launch fails, focus on entitlement and callback configuration.
- If the product opens but a workflow is blocked, move to the product permission or workflow troubleshooting path.

