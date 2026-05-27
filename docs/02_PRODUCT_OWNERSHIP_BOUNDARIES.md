# Product Ownership Boundaries

## Ownership Matrix

| Domain | Owner | Consumers |
|---|---|---|
| Platform login | NexArr | All products |
| Tenants | NexArr | All products |
| Product entitlement | NexArr | All products |
| Service clients/tokens | NexArr | Product APIs and workers |
| People and `personId` | StaffArr with NexArr login linkage | All products |
| Org/sites/departments/teams | StaffArr | All operational products |
| Permissions/certifications/readiness | StaffArr | TrainArr, MaintainArr, RoutArr, SupplyArr |
| Training programs/evidence | TrainArr | StaffArr and operational products |
| Training-derived qualifications | TrainArr issues, StaffArr publishes | Operational products |
| Assets/maintenance status | MaintainArr | RoutArr, SupplyArr, Compliance Core |
| Inspections/defects/work orders/PM | MaintainArr | StaffArr, RoutArr, Compliance Core |
| Routes/trips/DVIR/proof | RoutArr | StaffArr, MaintainArr, Compliance Core |
| Vendors/parts/inventory/purchasing | SupplyArr | MaintainArr, Compliance Core |
| Vocabulary/keys/rules/mappings/SDS | Compliance Core | All products |
| Public marketing | STLComplianceSite | Public visitors |

## Product Boundaries

### NexArr
Owns platform identity, login, tenants, entitlements, licensing, service clients, service tokens, platform admin, and launch.
Does not own operations, people records, maintenance, training, routing, purchasing, or compliance rule content.

### StaffArr
Owns people, org, roles, permissions, certifications, readiness, incidents, and personnel history.
Does not own login, entitlement, training workflow evidence, maintenance, routing, procurement, or rule packs.

### TrainArr
Owns programs, versions, requirements, assignments, evidence, tests, evaluations, signoffs, completions, retraining, and training-derived qualifications.
Does not own people/org truth, platform login, maintenance, routing, procurement, or compliance rule packs.

### MaintainArr
Owns assets, inspections, defects, work orders, PM, maintenance history, labor, part-consumption snapshots, and asset readiness.
Does not own people, training, dispatch, full procurement, vendors, or rule packs.

### RoutArr
Owns route planning, trip execution, dispatch, driver assignment, vehicle references, DVIR, proof, exceptions, and route history.
Does not own people, training, asset maintenance, procurement, or rule packs.

### SupplyArr
Owns vendors, suppliers, parts, catalogs, inventory, purchase requests, purchase orders, receiving, pricing, and lead times.
Does not own login, people, maintenance execution, training records, dispatch records, or rule packs.

### Compliance Core
Owns controlled vocabulary, compliance keys, material keys, rule packs, regulatory mappings, SDS/HazCom references, source metadata, and evaluation patterns.
Does not own tenant operations, people, assets, work orders, routes, purchase orders, or training workflow records.

### STLComplianceSite
Owns public content only. It has no tenant data, no operational workflow authority, and no product admin behavior.
