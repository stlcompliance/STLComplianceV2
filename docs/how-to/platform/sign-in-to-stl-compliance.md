# How to sign in to STL Compliance

## Audience
All users

## Product
NexArr

## Purpose
Enter the suite through the platform identity and tenant-membership authority.

## Before you start
- You need an active platform account and active membership in at least one tenant.
- Complete verification or MFA required by the tenant/security policy.

## Steps
1. Open the STL Compliance sign-in page.
2. Enter your email and password or approved identity-provider credentials.
3. Complete verification or MFA.
4. Select the tenant in which you intend to work when more than one is available.
5. Launch any ordinary product from the suite workspace or product switcher.

## What happens next
NexArr validates identity, active tenant membership, session security, and launch context. The destination product then validates product-local permissions, scope, workflow state, qualifications, and blockers.

## Troubleshooting
- If sign-in fails, confirm the account is active, unlocked, and using the correct sign-in method.
- If the tenant is missing, confirm active tenant membership.
- If an ordinary product is missing, check launcher cache, product registry/operational status, and handoff configuration.
- Compliance Core's administrative studio is visible only to platform administrators; its runtime evaluation still operates through ordinary product workflows.
