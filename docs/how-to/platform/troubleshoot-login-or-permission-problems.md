# How to troubleshoot login, launch, or permission problems

## Audience
Platform administrators, tenant administrators, StaffArr permission administrators, and support users

## Product
NexArr, StaffArr, and the affected product

## Purpose
Identify whether a problem is caused by account/session state, tenant membership, product operational state, launch handoff, product-local permissions, record scope, or workflow rules.

## Before you start
- Have the user, tenant, product, route, attempted action, approximate time, and error message.
- Platform-admin access is required only for NexArr security/launch diagnostics and the Compliance Core studio.

## Steps
1. Confirm the user is active, unlocked, and using the expected authentication method.
2. Confirm the user has active membership in the selected tenant.
3. Review session, risk, MFA, and recent security events.
4. Confirm the destination product is registered, active, healthy, and using an allowed callback/return URL.
5. Remember that every ordinary product is available to active tenant members; do not search for a product grant.
6. If the product opens, identify the exact missing page or action.
7. Review effective StaffArr permissions and their site/department/team/record scope.
8. Review product-owned workflow state, qualifications, holds, blockers, and segregation-of-duties rules.
9. Correlate the failed request through audit/correlation data without exposing raw diagnostics to the user.
10. Retest the same action and preserve the evidence of the fix.

## Outcome interpretation
- Sign-in failure: NexArr account/session/security problem.
- Tenant unavailable: NexArr tenant-membership problem.
- Ordinary product missing or launch failure: registry, operational-state, shell-cache, or handoff configuration problem.
- Compliance Core studio missing: expected unless the user is a platform administrator.
- Page/action unavailable after launch: StaffArr permission/scope or product-owned workflow rule.
- Data missing: tenant/filter/owner-service/degraded-read-model issue, not product availability.
