# I Am Missing a Permission

## Symptoms
- A button is hidden or disabled.
- A page opens read-only.
- An action returns a forbidden message.

## Likely causes
- Required product permission is missing.
- Role scope does not cover the site, department, team, record, or action.
- The authority projection is stale.
- Record status, qualification, segregation of duties, hold, or blocker prevents the action.

## What to check
1. Identify the exact product, route, record, and action.
2. Review effective StaffArr role/permission assignments and scope.
3. Review product-owned workflow and blocker rules.
4. Refresh the session/authority context after an approved change.

All ordinary products are available to active tenant members; product availability is not the permission being troubleshot.
