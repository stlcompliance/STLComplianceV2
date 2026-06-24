# Cross-Product Detail View Rule

All STL Compliance products must use a consistent detail-view pattern for primary records.

A detail view is the canonical read page for a single business record, such as a person, asset, supplier, training program, qualification, route, trip, work order, inspection, rule, mapping, finding, tenant, identity, or incident.

## Core intent

Every detail view must answer, without requiring navigation away from the page:

1. What is this record?
2. Who or what owns it?
3. What is its current state?
4. What decisions are currently allowed, watched, blocked, or pending?
5. What evidence supports that state?
6. What related records exist across products?
7. What actions can the current user take?
8. What changed recently?
9. What source of truth provides each major field?

## Layout standard

Detail views must use a content-first full-page layout.

Do not make the record detail a modal for primary business records.

A detail view must include:

- Page identity header
- Status / readiness / approval badges
- Primary action area
- Summary metric cards
- Main record snapshot
- Decision / readiness / compliance panel
- Related records section
- Documents / evidence section
- Recent activity / audit history section
- Product-specific tabs or sections
- Source-of-truth indicators where cross-product data is displayed

The page may include a right-side contextual rail for decision summaries, guidance, requirements, warnings, or workflow state, but the record must remain understandable without relying on hidden drawers or hover-only content.

## Header rule

The header must show:

- Product badge
- Record type
- Human-readable record title
- Stable record identifier
- Current lifecycle state
- Current operational decision, when applicable
- Primary ownership
- Primary location, site, department, or tenant context when applicable

Examples:

- StaffArr person: name, personId, active status, site, department, manager
- MaintainArr asset: unit number, assetId, readiness, location, assigned pool
- SupplyArr supplier: supplier name, supplierId, approval state, owner, risk tier
- TrainArr qualification: qualification name, qualificationId, current validity, owner
- RoutArr trip: trip number, tripId, dispatch state, driver, equipment
- Compliance Core rule: rule name, ruleId, governing body, applicability state

## Source-of-truth rule

Detail views must clearly distinguish owned data from referenced data.

A product may display cross-product data, but it must not imply ownership of data it does not own.

Where useful, fields should include source labels such as:

- NexArr source of truth
- StaffArr personId
- StaffArr org structure
- TrainArr qualification
- MaintainArr asset reference
- RoutArr trip reference
- SupplyArr supplier reference
- Compliance Core catalog
- Document evidence
- Calculated state
- User-entered note

Do not duplicate canonical data from another product unless the applicable constitution explicitly allows a snapshot.

Snapshots must be labeled as snapshots.

## Person identity rule

Any human shown in a detail view must be referenced by `personId`.

Do not use `userId` as the person source of truth.

Login capability must be shown separately from personhood.

Valid pattern:

- Person: `personId`
- Login: `hasUserAccount`
- Authority: active tenant/session context and local permission state

Invalid pattern:

- Treating a user account as the canonical human record
- Creating product-specific person identities
- Using free-text names where a StaffArr/NexArr person reference is required

## Field behavior

Detail views are read-only by default.

Editing must be an explicit mode entered through an action such as `Edit`, `Edit asset`, `Edit person`, `Edit supplier`, or `Manage access`.

When edit mode is enabled:

- Required fields must be obvious
- Controlled/selectable fields must be hydrated from backend fieldsets, catalogs, or reference providers
- Cross-product references must use search/select controls, not free text
- Destructive or state-changing edits must be permission-gated
- Save/cancel actions must be clear
- Unsaved changes must be protected

Do not expose raw IDs as the primary user-facing label unless the ID is operationally meaningful. IDs may appear as secondary metadata.

## Decision panel rule

Every detail view must include a decision panel when the record participates in readiness, compliance, approval, authorization, dispatch, purchasing, training, maintenance, or workflow routing.

The decision panel must summarize:

- Allowed state
- Watched state
- Blocked state
- Missing information
- Pending approvals
- Upcoming due dates
- Escalation or follow-up needs
- Reasoning in plain language

The decision panel must not expose raw rule JSON, system payloads, or internal evaluation blobs.

When a decision is based on rules, evidence, or cross-product references, show a plain-language explanation and link to the supporting records.

## Related-record rule

Detail views must expose related records in a structured section.

Related records must use typed links, not free-text references.

Examples:

- StaffArr incident linked to MaintainArr asset, RoutArr trip, TrainArr retraining, Compliance Core finding
- MaintainArr asset linked to work orders, inspections, defects, downtime, documents, SupplyArr parts
- SupplyArr supplier linked to purchase orders, parts, receiving, documents, approvals
- TrainArr assignment linked to personId, qualification, program, evaluation, remediation
- RoutArr trip linked to driver personId, asset, route, dispatch exception, incident
- Compliance Core rule linked to governing body, vocabulary term, mappings, findings, evaluations

If no related records exist, show an empty state with an allowed action to link or create a related record when the user has permission.

## Evidence and documents rule

Any detail view that relies on evidence must include a documents/evidence section.

Evidence must show:

- Document name
- Evidence type
- Status
- Date uploaded or effective date
- Expiration date when applicable
- Source product or upload source
- Linked requirement or business record when applicable

Evidence states should be clear:

- Current
- Expiring soon
- Expired
- Pending review
- Rejected
- Superseded
- Linked
- Missing

Do not bury evidence in audit history only.

## Activity and audit rule

Every detail view must include recent activity.

Audit-visible activity should show:

- Event type
- Plain-language event description
- Actor, using personId-backed display name when human
- Timestamp localized to the user
- Source product or service token when system-generated

Do not show raw event payloads in the normal detail view.

Raw technical audit data may exist behind an admin-only audit view, but the default detail page must remain human-readable.

## Tabs and sections rule

Tabs may be used when the record has multiple major dimensions.

Tabs must not hide critical state.

The Overview tab or top section must always include enough information to understand the record’s identity, state, ownership, decision, evidence health, and recent activity.

Common tab pattern:

- Overview
- Details
- Related records
- Documents
- History
- Reports or analytics, when applicable
- Settings, only when the record has configurable behavior

Product-specific examples:

- StaffArr person: Overview, Permissions, Certifications, Assignments, Incidents, Documents, History
- MaintainArr asset: Overview, Inspections, Work Orders, PM Plan, Defects, Documents, History
- SupplyArr supplier: Overview, Contacts, Documents, Items & Services, Purchase Orders, Performance, History
- TrainArr program: Overview, Steps, Assignments, Evaluations, Remediation, Citations, History
- RoutArr trip: Overview, Stops, Driver, Equipment, Exceptions, Documents, History
- Compliance Core rule: Overview, Applicability, Mappings, Findings, Evaluations, Citations, History

## Visual hierarchy rule

Detail views must prioritize comprehension over density.

Use consistent visual hierarchy:

1. Record identity
2. Current state and decision
3. Required action
4. Core fields
5. Related records
6. Evidence
7. History

Use badges, cards, and status colors consistently across products.

Recommended status color semantics:

- Green: allowed, current, ready, complete
- Amber: watched, due soon, needs review
- Red: blocked, expired, restricted, failed
- Blue: informational, linked, system-calculated
- Purple: training, qualification, evaluation, or specialized workflow
- Slate/gray: inactive, neutral, archived, not applicable

Do not rely on color alone; include text labels.

## Permission rule

Detail views must respect product-local permissions after NexArr validates identity, active tenant membership, and session/service context.

A user without edit permission may view allowed fields but must not see edit controls.

A user without permission for sensitive sections must see either:

- No section, when existence itself is sensitive
- A permission-limited placeholder, when the record can be known but details are restricted

Sensitive examples:

- HR notes
- medical details
- restricted incident details
- platform admin controls
- service tokens
- private documents
- disciplinary information

## Cross-product action rule

Actions shown on a detail view must be owned by the correct product.

A product may offer a cross-product action only by calling the owning product through an approved API, event, or service-token workflow.

Examples:

- StaffArr may forward training-related incident context to TrainArr
- MaintainArr may request SupplyArr parts demand but must not directly move inventory
- RoutArr may reference MaintainArr equipment readiness but must not edit the asset record
- SupplyArr may reference StaffArr sites but must not create canonical sites
- Compliance Core may evaluate evidence but must not become the operational owner of the underlying asset, person, trip, supplier, or work order

## Empty, loading, and error states

Detail views must provide clear states for:

- Loading record
- Record not found
- Permission denied
- Cross-product reference unavailable
- Evidence unavailable
- Source system timeout
- Archived or inactive record
- Deleted or superseded record

When a cross-product source is unavailable, show a clear warning and preserve locally safe snapshot data if allowed.

Do not silently hide failed reference data.

## No raw JSON rule

Detail views must not display raw JSON, internal rule payloads, webhook payloads, service-token claims, database rows, or event bodies to normal users.

System-generated reasoning must be translated into readable business language.

Admin/debug views may expose raw technical data only behind explicit permission gates.

## Implementation expectation

Every product detail view should be built from a shared detail-view design language, but each product owns its own domain-specific content.

Shared UI primitives may include:

- DetailPageShell
- RecordHeader
- StatusBadge
- DecisionPanel
- SourceField
- RelatedRecordList
- EvidenceList
- ActivityTimeline
- RequirementSummary
- PermissionGate
- EmptyState
- EditModeLayout

Shared primitives must not create shared business ownership.

The owning product remains responsible for its record model, permissions, API behavior, and lifecycle rules.

## Audit-aligned unified detail requirements

This constitution directly addresses UX-002 through UX-005, UI-001 through UI-004, and the audit’s partial-CRUD findings.

Detail pages are read-first and share one suite-wide hierarchy: identity and status, decision/readiness, required actions, key facts, related records, evidence/documents, recent activity, and deeper tabs. Product vocabulary may vary; the structural behavior may not.

Raw JSON, internal IDs, database values, permission keys, and developer linkage hints are not primary content. Human labels, plain-language reasoning, current owner/source, and meaningful codes come first. An explicit permissioned “Advanced technical details” disclosure may expose raw payloads when genuinely useful.

Every primary record must expose its real lifecycle actions and designed forbidden, not-found, archived, stale, conflict, owner-unavailable, and partial-data states. Related records use typed owner-backed links; historical snapshots are dated and labeled.

### Detail completion gate

Prove tenant and record scope, permission-aware actions, durable current state, server-owned transitions, owner-backed relationships, decision explanation, evidence and history, input preservation during edit conflicts, professional print/report where applicable, responsive/accessibility behavior, light/dark states, and list/drawer/detail route consistency.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
