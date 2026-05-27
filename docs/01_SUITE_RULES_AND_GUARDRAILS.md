# Suite Rules and Guardrails

## Non-Negotiable Rules


- Everything ships in V1: every product has a real API, worker, database, and suite UI surface.
- ARR means Adaptive Risk Reduction.
- NexArr owns login, tenants, platform identity, product entitlement, licensing, service clients, service tokens, and launch authority.
- StaffArr owns people, org structure, permissions, certifications, readiness, incidents, and personnel history.
- TrainArr owns training workflow, evidence, evaluations, signoffs, completions, retraining, recertification, and training-derived qualifications.
- MaintainArr owns assets, inspections, defects, work orders, preventive maintenance, maintenance history, and asset readiness.
- RoutArr owns routes, trips, dispatch, driver assignment, transportation execution, DVIR surfaces, proof, exceptions, and route history.
- SupplyArr owns vendors, dealers, suppliers, parts catalogs, purchasing, receiving, inventory, pricing snapshots, and lead-time snapshots.
- Compliance Core owns controlled vocabulary, regulatory keys, material keys, mappings, rule packs, SDS/HazCom references, and evaluation patterns.
- STLComplianceSite is marketing only.
- Suite Frontend is UI only and never grants business authority.
- Each product owns its own PostgreSQL database.
- No cross-product database foreign keys.
- No product directly mutates another product database.
- Cross-product relationships use APIs, events, service tokens, and local reference/mirror tables.
- Every API validates tenant context server-side.
- Every API validates NexArr identity and entitlement server-side.
- Every API enforces product-specific permissions server-side.
- Customer-hosted or external data is untrusted until validated by the owning service.
- Durable data belongs in PostgreSQL or object storage, not Render instance filesystem.
- Redis is cache and coordination only, never system-of-record storage.


## Naming Rules

- Product names: NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, STLComplianceSite.
- ARR means Adaptive Risk Reduction.
- API, UI, environment, and deployment names use consistent product keys.

## Boundary Rules

- A product can display another product's record through an owner-controlled API or embedded surface.
- A displayed external record never becomes local truth.
- A local mirror is rebuildable and clearly marked with source product, source ID, source event, and source timestamp.
- Product authority cannot live in shared UI components.
- Product authority cannot live in shared helper libraries.
- Product authority cannot live in the browser.

## Product Honesty Rules

- A feature exists only when API behavior, persistence, authorization, and UI flow exist.
- Mock screens, TODOs, seed examples, and frontend-only surfaces are not complete features.
- A thin feature can ship, but its server authority and ownership boundary must be real.

## Compliance Rules

- Compliance Core supplies keys, vocabulary, rules, mappings, and reason codes.
- Operational products supply facts and own workflow actions.
- Overrides require product permission and audit logging.
- Normal tenant users consume permitted compliance results through product APIs, not unrestricted rule-authoring surfaces.
