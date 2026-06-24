# Unified UI Page Coverage Matrix

## Purpose

This is the page-level completion contract for the suite. It prevents a product from appearing complete because it has a dashboard and a few routes while primary records lack consistent create, detail, lifecycle, evidence, failure, and navigation behavior.

## Required archetypes

For each primary record, mark whether these are required and link the route/component/test:

- index/list/board/queue
- preview drawer
- canonical detail page
- create page or guided intake workflow
- edit/delegated action page
- lifecycle actions with confirmation and permission checks
- related records and owner-backed references
- documents/evidence
- activity/timeline/history/audit
- print/report/export where operationally meaningful
- loading, empty/no-results, forbidden, not-found, stale, degraded, partial, conflict and error states

## Product coverage baseline

| Product | Primary records/workspaces requiring coverage |
|---|---|
| NexArr | tenants, memberships, accounts, sessions, registered products/operational states, launch attempts, service clients/tokens, platform admins, security audit events |
| StaffArr | people, org units, sites/locations, departments, positions, teams, role templates, assignments, permissions, incidents, delegated account actions, My Profile |
| TrainArr | programs, modules/lessons/steps, content, assignments, learning sessions, assessments, evaluations/signoffs, certificates, qualifications, remediation, renewals |
| MaintainArr | assets/components, reservations, defects, inspections/templates, PM plans/occurrences, work orders/tasks, labor, downtime, parts demand/use, readiness/overrides |
| SupplyArr | suppliers/vendors/dealers, contacts/locations, items/source relationships, sourcing events/quotes, purchase requests, purchase orders, contracts, compliance documents, scorecards, portal collaboration |
| LoadArr | expected receipts, dock appointments, receipts/lines, handling units, putaway tasks, inventory/balances/ledger, reservations, picks, staging/loadout, transfers/issues, counts/adjustments, holds/exceptions |
| AssurArr | nonconformances, holds, containment, dispositions, CAPAs/actions/effectiveness, audit programs/audits/findings, complaints, inspections/sampling, supplier quality issues, scorecards/releases |
| OrdArr | customer/internal requests, orders/lines, triage decisions, approvals, handoffs, holds, exceptions, returns/RMAs, completion packets, invoice/bill-ready packets |
| CustomArr | customers/accounts, hierarchies, contacts, locations, leads, opportunities, proposals, agreements, cases, activities/tasks, requirements/preferences, onboarding, portal identities, health/risk, imports/merge reviews |
| RoutArr | transportation demand, planning scenarios, tenders/routing guides, rates/accessorials, routes/trips/stops, dispatch board, visibility events, appointments, yard/gate/trailer events, exceptions, claims, proofs, finance contributions |
| RecordArr | records, files/versions, capture/scan jobs, OCR/extraction review, class/type/subtype, metadata, evidence mappings, packages, controlled documents, access policies/shares, retention, legal holds, purge review, access history |
| ReportArr | datasets/connectors/read models, dashboards/widgets, report definitions/versions, builder, runs/exports, schedules/recipients, metrics/KPIs, alerts, audit scopes/packages, lineage/provenance |
| LedgArr | financial legal entities, fiscal periods, accounts/dimensions, financial packets/mappings/previews, journals, vendor bills/matching/payments, invoices/receipts/applications, inventory valuation, fixed assets, projects, budgets, tax, reconciliations, external exports |
| Field Companion | My Work/task inbox, task detail/action, inspections, maintenance, warehouse, route stops/proofs, training, incident/evidence capture, secure upload, offline queue, conflicts/sync, device/session/privacy controls |
| Compliance Core studio | catalogs/vocabulary, governing bodies/jurisdictions/sources, citations, rulepacks/versions, requirements/fact needs, applicability, exceptions/exemptions, evidence mapping, questionnaires/profiles, evaluations/TSE, imports/review/publish, audit/diagnostics |
| STL Compliance Site | public pages, product/industry content, resources, leads/demo requests, contact inquiries, legal/trust/status content, redirects/SEO, publication history |

## Unified layout rules

- The same shell, page header, breadcrumb, action bar, filter region, table/list, drawer, dialog, toast, and state components are used across products.
- Product-specific identity may affect iconography/accent, not fundamental layout or interaction.
- One primary action appears in the page header; secondary actions live in an overflow menu or contextual action area.
- Tables show decision-useful defaults and avoid walls of columns. Dense details move to drawers/detail pages or optional columns.
- Create/edit forms use progressive sections, controlled references, quick create, preserved work, field-level errors, and server-confirmed success.
- Detail pages are read-first and place status, blockers, owner, and primary actions near the title.
- Drawers supplement, not replace, canonical detail pages.
- Every component and state is readable in light and dark through semantic tokens.

## Review gate

A product route-map PR must update this matrix. Missing primary-record coverage requires an explicit, temporary waiver with owner, reason, risk, target milestone, and a UI that does not pretend the missing workflow exists.
