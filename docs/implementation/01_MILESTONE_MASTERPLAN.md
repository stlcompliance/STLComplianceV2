# STL Compliance / Arr Suite Milestone Masterplan
## Milestone Overview
### M0 — Masterplan Lock and Feature Traceability

Goal: Freeze product ownership, feature IDs, acceptance definitions, API naming, and ship gates before coding accelerates.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M1 — Render and Repo Foundation

Goal: Provision full V1 Render resource shape, Dockerfiles, .NET 10 baselines, local Docker Compose, CI, health checks, migrations, and environment groups.
Primary feature coverage:
- **Compliance Core:** API surface /health.
- **MaintainArr:** API surface /health.
- **NexArr:** API surface /health.
- **RoutArr:** API surface /health.
- **StaffArr:** API surface /health.
- **SupplyArr:** API surface /health.
- **TrainArr:** API surface /health.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M2 — NexArr Platform Access Spine

Goal: Make every product launch, API call, and service-to-service call depend on NexArr identity, tenant, entitlement, and service-token trust.
Primary feature coverage:
- **NexArr:** auth login, logout, session renewal, session-renewal tokens, tenant management, tenant membership, product catalog, entitlement grants, entitlement revokes, service client registration, service token issuance, service token validation, product launch context, handoff codes, callback allowlist validation, tenant/product launch diagnostics, platform admin dashboard, launch failure states, plus 16 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M3 — Suite Frontend and Design System

Goal: Build the shared enterprise shell, React/Vite/Tailwind/lucide standards, product navigation, and UI foundations for every app.
Primary feature coverage:
- **STLComplianceSite:** homepage, NexArr page, StaffArr page, TrainArr page, MaintainArr page, RoutArr page, SupplyArr page, Compliance Core page, security page, data ownership page, demo/contact path, resources, pricing narrative, privacy/terms, SEO metadata, implementation maturity status, suite education content, client login CTA to NexArr, plus 1 more matrix rows.
- **Suite Frontend:** authenticated AppShell, product switcher, unified dashboard, NexArr surfaces, StaffArr surfaces, TrainArr surfaces, MaintainArr surfaces, RoutArr surfaces, SupplyArr surfaces, Compliance Core permitted surfaces, centralized lucide-react nav icon registry, server-driven entitlement navigation, server-driven permission hints, shared Tailwind 4 design system, shadcn/ui-style components, TanStack Query or RTK Query API layer, Zod validation, React Hook Form workflows, plus 2 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M4 — StaffArr Workforce Spine

Goal: Implement people, org, roles, permissions, incidents, readiness, and personnel audit foundations consumed by every product.
Primary feature coverage:
- **StaffArr:** people directory, person profile, person creation, onboarding flow, NexArr personId linkage, org tree, site assignments, department assignments, team assignments, position assignments, manager hierarchy, manager/subordinate view, role templates, permission templates, permission assignment, scoped permissions, permission history, certification visibility, plus 27 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M5 — Compliance Core Vocabulary and Rule Spine

Goal: Implement deterministic vocabulary, keys, citations, rule packs, facts, evaluations, findings, gates, and 9-CSV lifecycle.
Primary feature coverage:
- **Compliance Core:** vocabulary registry, alias mapping, 14 controlled vocabulary keys, material keys, compliance keys, governing body registry, jurisdiction registry, regulatory program registry, rule packs, rule versions, citation registry, citation versioning, fact catalog, fact source registry, fact requirements, regulatory mappings, 9-CSV import, 9-CSV export, plus 40 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M6 — TrainArr Qualification Spine

Goal: Implement training programs, requirements, assignments, evidence, signoffs, evaluations, qualifications, and StaffArr publication.
Primary feature coverage:
- **TrainArr:** tenant training dashboard, personal training dashboard, manager dashboard, trainer/evaluator dashboard, compliance dashboard, guided program builder, program type selection, program versioning, draft/review/publish lifecycle, requirement mapping, applicability builder, step builder, conditional branching, completion rule builder, result builder, publish review, assignment engine, assignment reasons, plus 28 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M7 — MaintainArr Maintenance Spine

Goal: Implement assets, meters, inspections, defects, work orders, PM, readiness, evidence, reports, and SupplyArr demand signals.
Primary feature coverage:
- **MaintainArr:** asset registry, asset creation, asset classification, asset lifecycle states, asset configuration, asset readiness calculation, meter tracking, usage tracking, meter correction workflow, PM forecast from usage, inspection template builder, versioned templates, dynamic inspections, inspection runner, mobile-first inspections, offline inspection capture, voice-guided inspection readiness, TTS prompts, plus 39 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M8 — SupplyArr Procurement Spine

Goal: Implement vendors, external parties, parts, documents, inventory, purchase requests, approvals, POs, receiving, and sourcing intelligence.
Primary feature coverage:
- **SupplyArr:** external party registry, vendor records, supplier records, dealer records, customer records, external party contacts, external party relationships, supplier onboarding, vendor approval status, vendor restrictions, supplier compliance documents, document upload, document metadata, document versioning, document expiration, document review, part catalog, materials catalog, plus 54 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M9 — RoutArr Dispatch Spine

Goal: Implement routes, dispatch, trips, stops, drivers, equipment, DVIR, proof, exceptions, incident forwarding, and audit trails.
Primary feature coverage:
- **RoutArr:** dispatch command center, daily dispatch board, weekly dispatch board, route calendar, driver availability panel, equipment availability panel, unassigned work queue, assigned trip list, active trip map/list, late trip highlighting, at-risk trip highlighting, exception queue, drag-and-drop assignment, bulk assignment actions, dispatch closeout, route planning, route templates, trip execution, plus 43 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M10 — Closed-Loop Cross-Product Workflows

Goal: Connect real flows: employee to qualified worker, asset to dispatch-ready, failed inspection to WO, WO parts demand, dispatch gates, incident to retraining, and audit packages.
Primary feature coverage:
- **MaintainArr:** asset readiness gate API, SupplyArr parts demand, purchase request creation through SupplyArr, RoutArr dispatchability summary, Compliance Core maintenance gates, TrainArr qualification checks before assignment, StaffArr technician references, SupplyArr demand event publisher.
- **RoutArr:** driver eligibility checks, StaffArr incident forwarding, Compliance Core dispatch gates, TrainArr qualification gates, MaintainArr equipment readiness gates, driver eligibility worker, API surface /api/driver-eligibility.
- **SupplyArr:** approval authority from StaffArr, MaintainArr demand intake, RoutArr demand intake, TrainArr/StaffArr demand intake, Compliance Core fact publishing.
- **TrainArr:** StaffArr publication, StaffArr acknowledgement tracking, authorization check API, batch qualification checks, citation attachment, rule-pack requirement intake, rule change impact, API surface /api/certification-publications.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M11 — Companion App Field Execution

Goal: Expose cross-product assigned work in a field-first app while every action still goes to the owning product API.
Primary feature coverage:
- **Companion App:** unified task inbox, product switcher for entitled products, MaintainArr assigned inspections, MaintainArr assigned work orders, RoutArr assigned trips, RoutArr DVIR tasks, TrainArr training assignments, SupplyArr receiving tasks, SupplyArr count tasks, SupplyArr approval tasks where permitted, StaffArr incidents and acknowledgements where permitted, photo evidence capture, document evidence capture, signature evidence capture, QR scan support, barcode scan support, offline-resilient task capture, clear submission state, plus 4 more matrix rows.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M12 — Reporting, Imports, Exports, and Audit Packages

Goal: Deliver proof-grade exports, reports, dashboards, import flows, evidence bundles, and audit packages across the suite.
Primary feature coverage:
- **Compliance Core:** source ingestion workflow, rule change monitoring, control effectiveness tracking, risk scoring, predictive missing-evidence warnings, readiness forecasting, vocabulary import worker, compliance key normalization worker, regulatory mapping validation worker, rule publication worker, SDS/HazCom maintenance worker.
- **MaintainArr:** maintenance reports, compliance reports, executive reports, imports, exports, audit logging, PM due-state worker, defect escalation worker, asset status rollup worker, maintenance history rollup worker.
- **NexArr:** audit export, service-token cleanup worker, entitlement reconciliation worker, tenant lifecycle worker.
- **RoutArr:** exception reporting, delay reporting, equipment issue reporting, incident reporting, reporting and analytics, route audit trail, driver dispatch history, equipment dispatch history, exportable audit packets, route state worker, trip completion rollup worker, DVIR follow-up worker, reference maintenance workers.
- **STLComplianceSite:** products hub, public capability accuracy labels, static SPA mechanics.
- **StaffArr:** audit package export, permission projection worker, certification expiration worker, personnel history rollup worker, audit package generation worker, API surface /api/audit-packages.
- **SupplyArr:** vendor reports, parts/inventory reports, purchasing reports, compliance reports, forgiving search, audit history, reorder worker, price snapshot worker, lead-time snapshot worker, procurement coordination worker, approval reminder worker, demand processing worker.
- **TrainArr:** person training history, training audit package, notification settings, expiration scanning worker, recertification assignment worker, qualification recalculation worker, StaffArr publish retry worker, event processing worker, notification dispatch worker, rule-pack impact worker, evidence retention worker, orphan reference detection worker.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
### M13 — Hardening, Performance, Security, and Ship Gate

Goal: Load, isolation, permissions, E2E journeys, OpenAPI parity, deployment checks, observability, recovery, and full acceptance completion.
Primary feature coverage:
- **NexArr:** health/readiness checks.

Acceptance:
- All planned APIs compile, run, and expose `/health` where applicable.
- All database changes are migration-backed and tenant-scoped.
- All protected actions validate NexArr identity, tenant, entitlement, and product permission.
- Frontend screens call real APIs or clearly stay outside the ship gate.
- Tests cover happy path, denied path, tenant isolation, idempotency, and audit where applicable.

---
