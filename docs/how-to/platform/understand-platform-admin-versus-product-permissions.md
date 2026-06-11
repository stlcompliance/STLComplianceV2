# How to understand platform admin versus product permissions

## Audience
Admins and product owners

## Product
Platform Access

## Support Status
Supported by current UI/API

## Purpose
Decide whether an access problem belongs in NexArr, StaffArr, or the product itself.

## Before You Start
- NexArr owns sign-in, tenant membership, product entitlement, platform roles, and launch.
- StaffArr owns people, internal places, permission assignments, and role assignments.
- Each product still validates actions in its own domain.

## Steps
1. For sign-in problems, check NexArr user state, locks, MFA, and sessions.
2. For missing products, check NexArr tenant entitlement and tenant membership.
3. For disabled product actions, check StaffArr role assignments and product-specific permission rules.
4. For missing source data such as people or locations, open the owning product instead of editing another product copy.
5. For cross-product issues, identify the product that owns the source record before changing anything.

## What Happens Next
This keeps each business truth in one place and prevents a product from becoming an accidental shadow owner.

## Troubleshooting
- If ownership is unclear, use the ownership constitution first.
- If a user has platform admin access but cannot perform a product workflow, check product-domain permissions before escalating as a platform issue.

