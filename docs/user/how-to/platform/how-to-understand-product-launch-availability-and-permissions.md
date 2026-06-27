# How to Understand Product Launch Status and Permissions

- Active tenant members can launch every ordinary STL Compliance product.
- NexArr controls identity, tenant membership, sessions, platform-admin status, product registry, and launch context.
- StaffArr records people, roles, permissions, and scope.
- Each product makes the final server-side decision for its actions and records.
- Compliance Core runtime operates through product workflows for all tenants; only its administrative studio is platform-admin-only.
- Missing access to an ordinary product is usually an operational-status, session, or launch-configuration problem, not a product-availability problem.
