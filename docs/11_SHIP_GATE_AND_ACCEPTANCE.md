# Ship Gate and Acceptance

## V1 Acceptance

V1 passes only when every product has:

- API service
- Worker service
- PostgreSQL database
- Health endpoint
- Auth, tenant, and entitlement enforcement
- Product-specific permission enforcement
- Suite frontend surface
- OpenAPI surface
- Audit/event posture
- All Features in FEATURESET docs are 100% implemented

## Infrastructure Gate

- Both static sites deploy.
- All seven APIs deploy and connect to their own databases.
- All seven workers deploy and connect to their own databases.
- Redis / Render Key Value exists.
- render.yaml defines the stack.

## Product Gate

| Product | Minimum V1 Proof |
|---|---|
| NexArr | tenant, product catalog, entitlement, service clients, launch context |
| StaffArr | person, org/site/team, role/permission, certification/readiness, history |
| TrainArr | program, version, assignment, evidence/signoff, qualification publication |
| MaintainArr | asset, inspection, defect, work order, PM, readiness endpoint |
| RoutArr | route/trip, driver and vehicle refs, dispatch assignment, DVIR/proof/exception |
| SupplyArr | vendor, part, inventory, purchase request, purchase order, receiving |
| Compliance Core | vocabulary, keys, material keys, rule pack, mapping, validate/resolve API |
| STLComplianceSite | public pages for each product, accurate ownership language, demo/contact path |

## Cross-Product Gate

Demonstrate:

- New employee to qualified worker
- Asset to dispatch-ready
- Failed inspection to work order
- Work order parts demand to SupplyArr request
- Training completion to StaffArr readiness
- Route assignment with driver and asset checks
- Compliance Core validation from an operational product

## Quality Gate

No frontend-only authority, no direct cross-product database access, no cross-product foreign keys, safe logs, health checks, common API errors, and sensitive-action audit records.
