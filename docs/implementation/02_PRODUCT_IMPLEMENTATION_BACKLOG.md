# Product Implementation Backlog
## STLComplianceSite

Type: Static Site

Ownership: Public marketing, public education, product positioning, contact/demo paths, public trust narrative.

### Feature Work
- [M3] homepage
- [M12] products hub
- [M3] NexArr page
- [M3] StaffArr page
- [M3] TrainArr page
- [M3] MaintainArr page
- [M3] RoutArr page
- [M3] SupplyArr page
- [M3] Compliance Core page
- [M3] security page
- [M3] data ownership page
- [M3] demo/contact path
- [M3] resources
- [M3] pricing narrative
- [M3] privacy/terms
- [M3] SEO metadata
- [M3] implementation maturity status
- [M12] public capability accuracy labels
- [M3] suite education content
- [M3] client login CTA to NexArr
- [M3] product launch CTAs through NexArr
- [M12] static SPA mechanics

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## Suite Frontend

Type: Static Site

Ownership: Authenticated suite UI, product switcher, entitlement-aware navigation, shared shell, shared design system.

### Feature Work
- [M3] authenticated AppShell
- [M3] product switcher
- [M3] unified dashboard
- [M3] NexArr surfaces
- [M3] StaffArr surfaces
- [M3] TrainArr surfaces
- [M3] MaintainArr surfaces
- [M3] RoutArr surfaces
- [M3] SupplyArr surfaces
- [M3] Compliance Core permitted surfaces
- [M3] centralized lucide-react nav icon registry
- [M3] server-driven entitlement navigation
- [M3] server-driven permission hints
- [M3] shared Tailwind 4 design system
- [M3] shadcn/ui-style components
- [M3] TanStack Query or RTK Query API layer
- [M3] Zod validation
- [M3] React Hook Form workflows
- [M3] Vitest unit coverage
- [M3] Playwright E2E coverage

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## NexArr

Type: API + Worker + DB

Ownership: Platform login, tenants, entitlements, product launch, service clients, service tokens, platform audit.

### Feature Work
- [M2] auth login
- [M2] logout
- [M2] session renewal
- [M2] session-renewal tokens
- [M2] tenant management
- [M2] tenant membership
- [M2] product catalog
- [M2] entitlement grants
- [M2] entitlement revokes
- [M2] service client registration
- [M2] service token issuance
- [M2] service token validation
- [M2] product launch context
- [M2] handoff codes
- [M2] callback allowlist validation
- [M2] tenant/product launch diagnostics
- [M2] platform admin dashboard
- [M2] launch failure states
- [M2] audit search
- [M12] audit export
- [M13] health/readiness checks
- [M2] hybrid data-plane metadata
- [M2] subscription/licensing records
- [M12] service-token cleanup worker
- [M12] entitlement reconciliation worker
- [M12] tenant lifecycle worker
- [M2] platform audit rollups

### API Work
- [M2] /api/auth/login
- [M2] /api/auth/renew
- [M2] /api/auth/logout
- [M2] /api/me
- [M2] /api/me/tenants
- [M2] /api/me/entitlements
- [M2] /api/me/navigation
- [M2] /api/tenants
- [M2] /api/products
- [M2] /api/entitlements
- [M2] /api/service-tokens
- [M2] /api/platform-admin/*
- [M1] /health

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## StaffArr

Type: API + Worker + DB

Ownership: People, org, sites, departments, teams, permissions, readiness, certifications visibility, personnel history, incidents.

### Feature Work
- [M4] people directory
- [M4] person profile
- [M4] person creation
- [M4] onboarding flow
- [M4] NexArr personId linkage
- [M4] org tree
- [M4] site assignments
- [M4] department assignments
- [M4] team assignments
- [M4] position assignments
- [M4] manager hierarchy
- [M4] manager/subordinate view
- [M4] role templates
- [M4] permission templates
- [M4] permission assignment
- [M4] scoped permissions
- [M4] permission history
- [M4] certification visibility
- [M4] certification grants/manual records
- [M4] manual overrides
- [M4] readiness calculation
- [M4] person readiness
- [M4] team readiness
- [M4] site readiness
- [M4] plain-English blockers
- [M4] incident intake
- [M4] incident routing to TrainArr
- [M4] training blocker display
- [M4] person timeline
- [M4] personnel notes
- [M4] documents
- [M12] audit package export
- [M4] product-facing readiness API
- [M4] product-facing person lookup API
- [M12] permission projection worker
- [M12] certification expiration worker
- [M12] personnel history rollup worker
- [M12] audit package generation worker

### API Work
- [M4] /api/people
- [M4] /api/org-units
- [M4] /api/sites
- [M4] /api/departments
- [M4] /api/teams
- [M4] /api/positions
- [M4] /api/roles
- [M4] /api/permissions
- [M4] /api/certifications
- [M4] /api/readiness
- [M4] /api/incidents
- [M4] /api/person-history
- [M12] /api/audit-packages
- [M1] /health

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## TrainArr

Type: API + Worker + DB

Ownership: Training programs, requirements, assignments, evidence, signoffs, evaluations, qualifications, publication to StaffArr.

### Feature Work
- [M6] tenant training dashboard
- [M6] personal training dashboard
- [M6] manager dashboard
- [M6] trainer/evaluator dashboard
- [M6] compliance dashboard
- [M6] guided program builder
- [M6] program type selection
- [M6] program versioning
- [M6] draft/review/publish lifecycle
- [M6] requirement mapping
- [M6] applicability builder
- [M6] step builder
- [M6] conditional branching
- [M6] completion rule builder
- [M6] result builder
- [M6] publish review
- [M6] assignment engine
- [M6] assignment reasons
- [M6] trainee completion flow
- [M6] training attempts
- [M6] evidence upload
- [M6] evidence verification
- [M6] trainer signoff
- [M6] evaluator signoff
- [M6] supervisor approval
- [M6] quiz/test steps
- [M6] practical evaluation steps
- [M6] remediation workflow
- [M6] recertification workflow
- [M6] qualification issue
- [M6] qualification suspend
- [M6] qualification revoke
- [M6] qualification expire
- [M10] StaffArr publication
- [M10] StaffArr acknowledgement tracking
- [M10] authorization check API
- [M10] batch qualification checks
- [M6] training matrix
- [M10] citation attachment
- [M10] rule-pack requirement intake
- [M10] rule change impact
- [M12] person training history
- [M12] training audit package
- [M12] notification settings
- [M6] integration settings
- [M12] expiration scanning worker
- [M12] recertification assignment worker
- [M12] qualification recalculation worker
- [M12] StaffArr publish retry worker
- [M12] event processing worker
- [M12] notification dispatch worker
- [M12] rule-pack impact worker
- [M12] evidence retention worker
- [M12] orphan reference detection worker

### API Work
- [M6] /api/training-definitions
- [M6] /api/training-programs
- [M6] /api/program-versions
- [M6] /api/training-requirements
- [M6] /api/training-assignments
- [M6] /api/training-evidence
- [M6] /api/signoffs
- [M6] /api/evaluations
- [M6] /api/completions
- [M6] /api/qualifications
- [M6] /api/qualification-checks
- [M10] /api/certification-publications
- [M1] /health

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## MaintainArr

Type: API + Worker + DB

Ownership: Assets, inspections, defects, work orders, PM, maintenance evidence, maintenance cost, asset readiness.

### Feature Work
- [M7] asset registry
- [M7] asset creation
- [M7] asset classification
- [M7] asset lifecycle states
- [M7] asset configuration
- [M7] asset readiness calculation
- [M10] asset readiness gate API
- [M7] meter tracking
- [M7] usage tracking
- [M7] meter correction workflow
- [M7] PM forecast from usage
- [M7] inspection template builder
- [M7] versioned templates
- [M7] dynamic inspections
- [M7] inspection runner
- [M7] mobile-first inspections
- [M7] offline inspection capture
- [M7] voice-guided inspection readiness
- [M7] TTS prompts
- [M7] STT constrained answers
- [M7] numeric voice normalization
- [M7] inspection history
- [M7] inspection analytics
- [M7] defect capture
- [M7] defect lifecycle
- [M7] defect severity
- [M7] defect intelligence
- [M7] work-order lifecycle
- [M7] work-order board
- [M7] task/job lines
- [M7] labor tracking
- [M7] evidence capture
- [M7] work-order completion
- [M7] repair verification
- [M7] PM program builder
- [M7] PM scheduling
- [M7] PM due-state evaluation
- [M7] auto WO generation
- [M7] auto inspection generation
- [M7] PM reset rules
- [M7] downtime tracking
- [M7] documents and attachments
- [M7] part-consumption snapshots
- [M10] SupplyArr parts demand
- [M7] part availability read-through
- [M7] part reservation request
- [M10] purchase request creation through SupplyArr
- [M7] received-part status display
- [M7] part cost snapshot
- [M10] RoutArr dispatchability summary
- [M10] Compliance Core maintenance gates
- [M10] TrainArr qualification checks before assignment
- [M10] StaffArr technician references
- [M12] maintenance reports
- [M12] compliance reports
- [M12] executive reports
- [M12] imports
- [M12] exports
- [M12] audit logging
- [M12] PM due-state worker
- [M12] defect escalation worker
- [M12] asset status rollup worker
- [M12] maintenance history rollup worker
- [M10] SupplyArr demand event publisher

### API Work
- [M7] /api/assets
- [M7] /api/asset-classes
- [M7] /api/asset-types
- [M7] /api/inspections
- [M7] /api/inspection-templates
- [M7] /api/defects
- [M7] /api/work-orders
- [M7] /api/preventive-maintenance
- [M7] /api/maintenance-history
- [M7] /api/asset-readiness
- [M7] /api/technician-refs
- [M1] /health

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## RoutArr

Type: API + Worker + DB

Ownership: Dispatch, routes, trips, stops, driver/equipment assignment, proof, exceptions, transportation audit trail.

### Feature Work
- [M9] dispatch command center
- [M9] daily dispatch board
- [M9] weekly dispatch board
- [M9] route calendar
- [M9] driver availability panel
- [M9] equipment availability panel
- [M9] unassigned work queue
- [M9] assigned trip list
- [M9] active trip map/list
- [M9] late trip highlighting
- [M9] at-risk trip highlighting
- [M9] exception queue
- [M9] drag-and-drop assignment
- [M9] bulk assignment actions
- [M9] dispatch closeout
- [M9] route planning
- [M9] route templates
- [M9] trip execution
- [M9] start trip
- [M9] complete trip
- [M9] close trip
- [M9] cancel trip
- [M9] stop management
- [M9] stop sequencing
- [M9] arrive stop
- [M9] complete stop
- [M9] driver portal
- [M9] today assigned trips
- [M9] upcoming trips
- [M9] navigation handoff
- [M9] pickup confirmation
- [M9] delivery confirmation
- [M9] equipment assignment
- [M9] driver assignment
- [M10] driver eligibility checks
- [M9] asset dispatchability checks
- [M9] load/movement records
- [M9] DVIR
- [M9] pre-trip DVIR
- [M9] post-trip DVIR
- [M9] proof capture
- [M9] photos/documents/signatures
- [M9] proof archive
- [M12] exception reporting
- [M12] delay reporting
- [M12] equipment issue reporting
- [M12] incident reporting
- [M10] StaffArr incident forwarding
- [M10] Compliance Core dispatch gates
- [M10] TrainArr qualification gates
- [M10] MaintainArr equipment readiness gates
- [M9] short-haul/time tracking support
- [M9] communication and notes
- [M9] messaging with dispatch
- [M9] driver performance summary
- [M9] route health dashboard
- [M9] driver/equipment utilization
- [M12] reporting and analytics
- [M12] route audit trail
- [M12] driver dispatch history
- [M12] equipment dispatch history
- [M12] exportable audit packets
- [M9] cross-product events
- [M9] admin/configuration
- [M12] route state worker
- [M12] trip completion rollup worker
- [M10] driver eligibility worker
- [M12] DVIR follow-up worker
- [M12] reference maintenance workers

### API Work
- [M9] /api/routes
- [M9] /api/dispatch
- [M9] /api/trips
- [M9] /api/stops
- [M9] /api/drivers
- [M10] /api/driver-eligibility
- [M9] /api/vehicle-refs
- [M9] /api/dvir
- [M9] /api/route-inspections
- [M9] /api/proof
- [M9] /api/exceptions
- [M9] /api/route-completions
- [M1] /health

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## SupplyArr

Type: API + Worker + DB

Ownership: External parties, vendors, suppliers, parts, inventory, procurement, receiving, supplier docs, supply readiness.

### Feature Work
- [M8] external party registry
- [M8] vendor records
- [M8] supplier records
- [M8] dealer records
- [M8] customer records
- [M8] external party contacts
- [M8] external party relationships
- [M8] supplier onboarding
- [M8] vendor approval status
- [M8] vendor restrictions
- [M8] supplier compliance documents
- [M8] document upload
- [M8] document metadata
- [M8] document versioning
- [M8] document expiration
- [M8] document review
- [M8] part catalog
- [M8] materials catalog
- [M8] service-purchasing catalog
- [M8] manufacturer records
- [M8] manufacturer aliases
- [M8] internal part numbers
- [M8] vendor part numbers
- [M8] part aliases
- [M8] part equivalents
- [M8] supersessions
- [M8] substitutions
- [M8] UOM support
- [M8] category support
- [M8] pricing snapshots
- [M8] lead-time snapshots
- [M8] availability snapshots
- [M8] preferred source recommendations
- [M8] inventory locations
- [M8] bins
- [M8] stock counts
- [M8] reservations
- [M8] transfers
- [M8] cycle counts
- [M8] reorder points
- [M8] reorder evaluation
- [M8] purchase request workflow
- [M8] approval workflow
- [M10] approval authority from StaffArr
- [M8] RFQs
- [M8] quote comparison
- [M8] purchase order workflow
- [M8] PO issue
- [M8] PO cancellation
- [M8] receiving
- [M8] receiving exceptions
- [M8] backorders
- [M8] returns
- [M8] warranty claims
- [M8] emergency purchase workflow
- [M10] MaintainArr demand intake
- [M8] demand refs
- [M10] RoutArr demand intake
- [M10] TrainArr/StaffArr demand intake
- [M8] supplier incidents
- [M8] procurement exceptions
- [M10] Compliance Core fact publishing
- [M8] supply readiness dashboard
- [M12] vendor reports
- [M12] parts/inventory reports
- [M12] purchasing reports
- [M12] compliance reports
- [M12] forgiving search
- [M12] audit history
- [M8] event outbox/inbox
- [M12] reorder worker
- [M12] price snapshot worker
- [M12] lead-time snapshot worker
- [M12] procurement coordination worker
- [M12] approval reminder worker
- [M12] demand processing worker

### API Work
- [M8] /api/vendors
- [M8] /api/dealers
- [M8] /api/suppliers
- [M8] /api/parties
- [M8] /api/parts
- [M8] /api/catalogs
- [M8] /api/inventory
- [M8] /api/purchase-requests
- [M8] /api/purchase-orders
- [M8] /api/receiving
- [M8] /api/pricing-snapshots
- [M8] /api/lead-time-snapshots
- [M8] /api/demand-refs
- [M1] /health

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## Compliance Core

Type: API + Worker + DB

Ownership: Controlled vocabulary, compliance keys, material keys, rule packs, citations, mappings, deterministic evaluation, findings, waivers, audit packages.

### Feature Work
- [M5] vocabulary registry
- [M5] alias mapping
- [M5] 14 controlled vocabulary keys
- [M5] material keys
- [M5] compliance keys
- [M5] governing body registry
- [M5] jurisdiction registry
- [M5] regulatory program registry
- [M5] rule packs
- [M5] rule versions
- [M5] citation registry
- [M5] citation versioning
- [M5] fact catalog
- [M5] fact source registry
- [M5] fact requirements
- [M5] regulatory mappings
- [M5] 9-CSV import
- [M5] 9-CSV export
- [M5] CSV preview/dry run
- [M5] any/all/none rule builder
- [M5] condition operators
- [M5] missing/stale fact handling
- [M5] rule tests
- [M5] simulator
- [M5] draft/review/publish lifecycle
- [M5] rule diff viewer
- [M5] change impact analysis
- [M5] deterministic evaluation API
- [M5] evaluation snapshots
- [M5] plain-English explanations
- [M5] trace output
- [M5] workflow gate API
- [M5] allow/warn/block/review outcomes
- [M5] remediation guidance
- [M5] evidence requirements
- [M5] hard-block rules
- [M5] non-waivable rules
- [M5] waiver-aware evaluations
- [M5] findings
- [M5] exception/waiver reports
- [M5] evaluation history explorer
- [M5] tenant/domain/product compliance dashboards
- [M5] cross-product audit package
- [M5] SDS references
- [M5] HazCom references
- [M5] regulatory-class-to-facet derivations
- [M12] source ingestion workflow
- [M12] rule change monitoring
- [M5] citation supersession tracking
- [M12] control effectiveness tracking
- [M12] risk scoring
- [M12] predictive missing-evidence warnings
- [M12] readiness forecasting
- [M12] vocabulary import worker
- [M12] compliance key normalization worker
- [M12] regulatory mapping validation worker
- [M12] rule publication worker
- [M12] SDS/HazCom maintenance worker

### API Work
- [M5] /api/vocabulary
- [M5] /api/compliance-keys
- [M5] /api/material-keys
- [M5] /api/regulatory-mappings
- [M5] /api/rule-packs
- [M5] /api/rule-versions
- [M5] /api/sds
- [M5] /api/hazcom
- [M5] /api/findings
- [M5] /api/internal/resolve
- [M5] /api/internal/validate
- [M1] /health

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
## Companion App

Type: Mobile App

Ownership: Field-first task experience that calls owning product APIs and does not become a source of truth.

### Feature Work
- [M11] unified task inbox
- [M11] product switcher for entitled products
- [M11] MaintainArr assigned inspections
- [M11] MaintainArr assigned work orders
- [M11] RoutArr assigned trips
- [M11] RoutArr DVIR tasks
- [M11] TrainArr training assignments
- [M11] SupplyArr receiving tasks
- [M11] SupplyArr count tasks
- [M11] SupplyArr approval tasks where permitted
- [M11] StaffArr incidents and acknowledgements where permitted
- [M11] photo evidence capture
- [M11] document evidence capture
- [M11] signature evidence capture
- [M11] QR scan support
- [M11] barcode scan support
- [M11] offline-resilient task capture
- [M11] clear submission state
- [M11] idempotency keys
- [M11] push-notification readiness
- [M11] plain blocked/denied reason messages
- [M11] server-side submission validation

### Product Completion Gate
- Domain records are owned only by this product.
- Cross-product data is referenced through stable IDs, local mirrors, API reads, or event snapshots.
- UI, API, worker, DB migrations, OpenAPI, tests, audit logging, and reports exist for every shipped claim.

---
