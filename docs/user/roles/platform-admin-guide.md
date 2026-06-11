# Platform Admin Guide

## What This Role Does
A platform administrator manages NexArr platform access, tenant and product visibility, launch diagnostics, platform sessions, platform status, reference data, entitlement reconciliation, and platform audit exports.

## What This Role Can Usually Access
- NexArr platform administration pages when the account has platform admin access.
- Suite dashboard and product switcher for entitled products.
- Platform audit export and launch diagnostics.

## What This Role Usually Cannot Access
- Does not automatically own product records such as work orders, training assignments, trips, inventory, or records.
- Should not correct source data in ReportArr; corrections happen in the owning product.
- Should not bypass product approvals or audit requirements.

## Common Daily Tasks
- Review launch failures.
- Check tenant, user, and product access.
- Review platform status.
- Export platform audit packages.
- Help tenant admins resolve missing product access.

## Records This Role Works With
- tenant
- user account
- product entitlement
- launch attempt
- platform audit event

## Notifications This Role May Receive
- Launch failures
- platform health warnings
- session or entitlement issues
- audit export completion or failure

## Common Issues
- User can sign in but sees no products.
- Product launch fails.
- Platform admin page is not visible because the account is not platform admin.
- Remote deployment does not change until changes are committed and pushed.

## Related How-To Documents
- [How to understand product access](../how-to/platform/how-to-understand-product-access.md)
- [How to troubleshoot missing product access](../how-to/platform/how-to-troubleshoot-missing-product-access.md)
