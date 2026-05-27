# STL Compliance / Arr Suite Masterplan Docs

This package defines the STL Compliance / Arr ecosystem as a clean masterplan.

## Document Map

- `00_STL_COMPLIANCE_MASTERPLAN.md` — suite vision and operating model
- `01_SUITE_RULES_AND_GUARDRAILS.md` — non-negotiable rules
- `02_PRODUCT_OWNERSHIP_BOUNDARIES.md` — ownership matrix and boundaries
- `03_RENDER_FULL_V1_DEPLOYMENT_PLAN.md` — Render resource plan
- `04_RUNTIME_FRAMEWORKS_AND_PACKAGES.md` — .NET 10, React, Vite, PostgreSQL, Redis, Lucide
- `05_FRONTEND_UI_AND_DESIGN_SYSTEM.md` — app shell, icons, layout, accessibility
- `06_AUTH_SECURITY_PERMISSIONS_AND_ROLES.md` — auth, security, default roles, permission keys
- `07_DATA_OWNERSHIP_AND_DATABASE_DESIGN.md` — product databases and local references
- `08_EVENTS_WORKERS_AND_INTEGRATION.md` — outbox, workers, events, integrations
- `09_API_CONVENTIONS_AND_SERVICE_CONTRACTS.md` — API patterns and contracts
- `10_REPO_STRUCTURE_LOCAL_DEV_AND_CI.md` — monorepo, local ports, CI
- `11_SHIP_GATE_AND_ACCEPTANCE.md` — V1 acceptance gate
- `12_NEXARR_FEATURESET.md` through `19_STLCOMPLIANCESITE_FEATURESET.md` — product feature sets
- `20_COMPANION_APP_FEATURESET.md` — field/mobile app concept
- `21_PERMISSION_KEYS_AND_DEFAULT_ROLES.md` — permission and role catalog
- `22_CONTROLLED_VOCABULARY_AND_COMPLIANCE_KEYS.md` — Compliance Core keys and CSV logic
- `23_CROSS_PRODUCT_WORKFLOWS.md` — suite workflows
- `24_DESIGN_DECISION_LOG.md` — decisions and reasons

## Core Rules


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
