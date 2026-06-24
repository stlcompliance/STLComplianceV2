# Cross-Product Create View Rule

All STL Compliance products must use a consistent guided create-view pattern for primary business records.

A create view is the canonical page for creating a new primary record, such as a person, asset, supplier, training program, qualification, route, trip, work order, inspection, rule, mapping, finding, tenant, identity, incident, purchase order, or other product-owned business object.

Create views must be full-page workflows, not modals, when the record is a primary business object or has cross-product effects.

## Core intent

Every create view must help the user create a valid, owned, auditable record without overwhelming them.

The create workflow must answer:

1. What am I creating?
2. Which product owns this record?
3. What fields are required before the record can exist?
4. Which fields are controlled/selectable instead of free text?
5. Which cross-product references are required?
6. What sections become available after the minimum valid record exists?
7. What will happen when I save, submit, publish, assign, approve, or launch this record?
8. What evidence or documents are needed?
9. What decisions, readiness states, permissions, or follow-up workflows may be triggered?

## Progressive section rule

Create views must use progressive section expansion.

Only the first required section is expanded when the page loads.

Each next section remains collapsed, disabled, or visually locked until the previous required section is complete enough to validate.

When a section is completed, the next section expands automatically.

Completed sections collapse into a readable summary card, unless the user chooses to reopen them.

A user may reopen completed sections to edit earlier answers, but changing an earlier answer may invalidate later sections.

When earlier edits invalidate later data, the UI must clearly mark affected downstream sections as needing review.

Do not show all create sections fully expanded at once unless the object is extremely simple and has no downstream effects.

## Section states

Each create section must have a visible state.

Allowed states:

- Not started
- Locked
- In progress
- Needs required fields
- Needs review
- Complete
- Invalidated
- Optional
- Skipped
- Submitted

The section header must show the section title, state, and short completion summary.

Example:

- Incident Basics — Complete — “Minor safety incident at Sparta Operations Center”
- People & Involvement — Needs required fields — “Reporter required”
- Training Review — Locked — “Complete readiness impact first”
- Evidence — Optional — “No files attached”

## Standard create flow

A primary create view should generally follow this order:

1. Identity / basics
2. Ownership / source of truth
3. Required classification
4. Required people, asset, site, supplier, route, or rule references
5. Narrative, description, or scope
6. Decision-impact fields
7. Related records
8. Evidence and documents
9. Notifications, routing, or workflow
10. Review and submit

Product-specific flows may rename sections, but the ordering principle remains:

Minimum valid identity first.
Classification and ownership second.
Operational details third.
Decision and workflow effects fourth.
Evidence and review last.

## First section rule

The first section must collect the minimum fields required for the record to exist as a draft.

The first section should usually include:

- Record title or name
- Record type
- Owning product context
- Tenant context when applicable
- Site, department, or org context when applicable
- Primary owner or responsible person when applicable
- Required classification
- Initial status

After the first section validates, the system may create a draft record ID.

Draft IDs are allowed, but the UI must clearly show that the record is not final, active, published, approved, or submitted until the workflow is complete.

## Draft behavior

Create views must support saving a draft when the record is complex.

A draft may exist before all sections are complete, but it must be clearly labeled as Draft.

Drafts must preserve completed section data.

Drafts must preserve validation state.

Drafts must not trigger final workflows unless the user explicitly submits, publishes, approves, launches, or activates the record.

Examples of actions that should not occur on simple draft save:

- Assigning training
- Restricting a person
- Dispatching a route
- Opening a work order
- Moving inventory
- Publishing a rule
- Sending compliance notifications
- Starting an approval route
- Sending external emails
- Creating audit-final evidence packages

## Controlled field rule

Create views must prefer controlled/selectable fields over free text.

Use backend fieldsets, catalogs, reference providers, and owning-product APIs wherever possible.

Examples:

- Person references must use personId-backed search/select
- Sites must come from StaffArr org structure
- Governing bodies must come from Compliance Core catalogs
- Asset references must come from MaintainArr
- Route/trip references must come from RoutArr
- Supplier/party references must come from SupplyArr
- Training/qualification references must come from TrainArr
- Tenant, membership, session, and platform-admin references must come from NexArr

Free text is allowed only for narrative, notes, descriptions, and details that are not controlled business entities.

## Cross-product reference rule

Cross-product references must be selected, not typed.

A create view may reference another product’s record only through approved APIs, events, service-token workflows, or reference providers.

The creating product must not duplicate ownership of the referenced record.

Examples:

- StaffArr incident create may reference a MaintainArr asset
- StaffArr incident create may reference a RoutArr trip
- StaffArr incident create may route training review to TrainArr
- MaintainArr work order create may request SupplyArr part demand
- SupplyArr purchase create may reference StaffArr site identity
- Compliance Core mapping create may reference evidence from another product

The UI must label cross-product fields with their source.

## Completion gating

A section is complete only when:

- Required fields are present
- Field values pass validation
- Cross-product references resolve successfully
- Permission checks pass
- Required controlled selections use valid catalog values
- Required dates, statuses, and ownership fields are valid
- The section has no blocking errors

A section may be allowed to complete with warnings, but warnings must be visible in the section summary and final review.

Warnings do not block progression unless the product’s rules say they must.

## Collapsed completed section summary

When a section is complete and collapses, it must show a useful summary.

Do not collapse completed sections into only a checkmark.

Good summaries:

- “Asset Basics — Complete — TRK-1042, Semi Tractor, Sparta Operations Center”
- “People — Complete — Affected: Marcus Hill, Reporter: Kelsey Martin”
- “Supplier Basics — Complete — Midwest Fleet Parts, Approved vendor candidate”
- “Training Impact — Complete — TrainArr review required”
- “Evidence — Complete — 3 files attached, 1 pending review”

## Invalidated section rule

When a user edits an earlier section, downstream sections may become invalid.

The UI must detect and show this.

Examples:

- Changing incident severity may invalidate readiness, notifications, and review routing
- Changing affected person may invalidate manager, training, permissions, and readiness
- Changing asset class may invalidate inspection template, PM plan, and required fields
- Changing supplier category may invalidate required documents and approval route
- Changing governing body may invalidate rulepack, citations, and mappings

Invalidated sections must show:

- What changed
- Which downstream section needs review
- Whether the section is blocked or only warned
- What action the user must take

## Review and submit rule

The final section must be a Review & Submit section.

It must not unlock until all required sections are complete or intentionally skipped where skipping is allowed.

The final section must show:

- Completion checklist
- Required-field status
- Warnings
- Blockers
- Cross-product effects
- Notifications that will be sent
- Workflows that will be started
- Records that will be linked
- Evidence package status
- Final user actions

Common final actions:

- Save draft
- Submit
- Publish
- Activate
- Assign
- Approve
- Cancel

The final action label must match the business effect.

Do not use vague labels like “Done” for state-changing actions.

## Submit behavior

Submitting a create workflow may trigger business effects.

The UI must clearly explain those effects before submission.

Examples:

- StaffArr incident submit may notify manager and safety
- StaffArr incident submit may queue TrainArr evaluation
- MaintainArr asset create may activate PM eligibility
- MaintainArr work order create may create parts demand
- TrainArr assignment create may notify trainee and evaluator
- SupplyArr supplier create may start approval workflow
- RoutArr trip create may reserve driver/equipment availability
- Compliance Core rule create may create an unpublished rule draft

## Visual layout rule

Create views must be content-first and full-page.

The recommended layout is:

- Page header with product badge, create title, draft/submission state, and primary actions
- Summary cards showing required fields, workflow effect, decision impact, and completion state
- Main guided section column
- Optional right-side contextual rail
- Final review section at the bottom

The right-side rail may show:

- Workflow steps
- Source-of-truth guidance
- Completion status
- Warnings
- Help text
- Related policy links
- Draft state
- Permission notes

The right rail must not contain required fields that are easy to miss.

## No modal rule

Primary record creation must not happen in a small modal when the workflow has:

- More than one required section
- Cross-product references
- Evidence uploads
- Approval routing
- Readiness impact
- Training impact
- Compliance impact
- Permission impact
- Dispatch impact
- Purchasing impact
- Audit history impact

Small modals are allowed only for simple supporting records with no major workflow effect.

## Permission rule

Create sections and actions must be permission-gated.

A user may only use create actions allowed by product-local permissions after NexArr validates identity, active tenant membership, and session/service context.

If a section is unavailable due to permission, show an appropriate permission-limited state.

Do not show hidden sensitive fields as disabled placeholders if the field’s existence is itself sensitive.

## Evidence rule

Evidence sections should appear after the record has enough identity and classification to attach evidence correctly.

Evidence uploads must show:

- File name
- File type
- Upload state
- Review state
- Linked requirement or record
- Expiration or effective date when applicable
- Whether the evidence will be included in the audit package

Evidence must not be buried only in notes or history.

## Activity rule

Create views may show draft activity once a draft exists.

Draft activity should show:

- Draft created
- Section completed
- Section reopened
- Section invalidated
- Evidence uploaded
- Cross-product reference linked
- Reviewer assigned
- Submission completed

Do not show raw event payloads to normal users.

## No raw JSON rule

Create views must not expose raw JSON, webhook payloads, rule payloads, service-token claims, database rows, or internal system payloads to normal users.

System logic must be translated into plain business language.

## Product examples

### StaffArr incident create

Recommended progressive sections:

1. Incident Basics
2. People & Involvement
3. Narrative & Details
4. Readiness / Restriction Impact
5. Training / Certification Evaluation
6. Related Records & Cross-Product Links
7. Evidence & Attachments
8. Notifications & Workflow
9. Review & Submit

Each section expands only after the previous section is complete.

Training review must not unlock until incident type, severity, affected person, and readiness impact are known.

Review & Submit must not unlock until all required sections are complete.

### MaintainArr asset create

Recommended progressive sections:

1. Required Asset Identity
2. Classification & Configuration
3. Site / Ownership / Assignment
4. Meters & Operating Counters
5. Compliance Applicability
6. PM / Inspection Eligibility
7. Documents & Evidence
8. Review & Create Asset

Asset class and configuration must drive later required fields.

### SupplyArr supplier create

Recommended progressive sections:

1. Supplier Identity
2. Supplier Type & Category
3. Contacts
4. Terms & Tax
5. Required Documents
6. Approval / Risk Review
7. Linked Items or Services
8. Review & Submit Supplier

Supplier type must drive document and approval requirements.

### TrainArr program create

Recommended progressive sections:

1. Program Identity
2. Governing Body / Rulepack
3. Audience & Applicability
4. Steps / Lessons / Evaluations
5. Signoff Requirements
6. Remediation Rules
7. Evidence / Certificate Output
8. Review & Publish Program

Program cannot publish until applicability, steps, signoffs, and completion rules are valid.

### RoutArr trip create

Recommended progressive sections:

1. Trip Identity
2. Route / Stops
3. Driver
4. Equipment
5. Availability / Readiness Checks
6. Documents / Special Requirements
7. Dispatch Review
8. Create Trip

Driver and equipment sections must validate readiness before dispatch.

### Compliance Core rule create

Recommended progressive sections:

1. Rule Identity
2. Governing Body / Citation
3. Applicability
4. Evidence Requirements
5. Evaluation Logic
6. Mappings
7. Findings / Output Behavior
8. Review & Publish Rule

Evaluation logic must remain human-readable and must not expose raw JSON as the default interface.

## Implementation expectation

Create workflows should be implemented using reusable UI primitives but product-owned business logic.

Recommended shared primitives:

- CreatePageShell
- ProgressiveSection
- SectionSummary
- SectionCompletionState
- SourceField
- ControlledSelect
- ReferenceSearch
- EvidenceUpload
- DecisionPreview
- WorkflowPreview
- ReviewChecklist
- PermissionGate
- InvalidatedSectionNotice
- DraftActionBar

Shared primitives must not create shared business ownership.

Each product remains responsible for its own record model, permissions, validation, lifecycle, API behavior, and submission effects.

## Non-negotiable rule

A create view must progressively expand one section at a time, using completed prior sections to hydrate, validate, unlock, or invalidate later sections, so users are guided through the minimum valid path before optional or downstream details are shown.

## Audit-aligned unified create and edit requirements

This constitution also governs edit forms and responds to FUNC-001, UX-001, UX-004, UX-005, and UI-001 through UI-004.

Create/edit pages must use the shared page header, section, field, reference picker, quick-create drawer, validation summary, action bar, dialog, and page-state patterns. They must not become database-column dumps, giant undifferentiated forms, or walls of instructional text.

Recommended information order:

1. required basics needed to create a useful valid record
2. ownership/context and owner-backed references
3. operational details
4. compliance/evidence requirements
5. optional or later-backfill details
6. review and durable submit

Cross-product references must use live owner-backed search/select controls. When a required reference is missing, a permissioned quick-create flow may create the minimum valid owner record and return it to the form without discarding current work.

Success is shown only after durable server confirmation. Recoverable validation, permission, conflict, or dependency failures preserve entered data and explain what was not saved. A frontend may not create a local substitute or success state after an API failure.

### Create/edit completion gate

Prove tenant and permission scope, actor derivation, required/optional clarity, owner-backed references, quick create, server validation, concurrency, idempotent submit where needed, failure preservation, dirty-state handling, accessible keyboard/focus behavior, responsive layout, light/dark states, and regression tests.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
