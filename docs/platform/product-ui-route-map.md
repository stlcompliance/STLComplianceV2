# STL Compliance Product UI Route Map

## Purpose

This document maps product ownership and page archetypes into a unified, practical route system. A route may display cross-product context, but edits occur through the owning product or an owner-backed delegated action.

## Shared route rules

Every ordinary product provides a stable home, search where useful, My Work/task access, product-local reports where justified, preferences, settings where authorized, and audit/history where authorized. Route names use durable business language rather than implementation labels.

```text
/{product}/
/{product}/search
/{product}/tasks
/{product}/reports
/{product}/preferences
/{product}/settings
/{product}/audit
```

Not every link belongs in the sidebar. Product navigation is grouped by workflow, keeps common destinations visible, and moves uncommon administration into Settings or Administration.

## Platform shell

```text
/
  product launcher containing all active ordinary products
  cross-product My Work and notifications
  global search where authorized
  current tenant and product context
  profile, preferences, session, help

/admin
  NexArr-owned platform administration only

/compliancecore
  administrative studio; server-validated platform administrators only

/platform/reference-data
  platform-admin operational utility; not a tenant product or launcher item
```

Opening an ordinary product requires an authenticated account, active tenant membership, safe session, and active destination. Product-local permissions, record scope, workflow state, qualifications, and blockers govern actions after launch.

## Product route groups

| Product | Primary route groups |
|---|---|
| NexArr | `/nexarr/tenants`, `/nexarr/memberships`, `/nexarr/accounts`, `/nexarr/sessions`, `/nexarr/products`, `/nexarr/product-status`, `/nexarr/product-launch`, `/nexarr/service-clients`, `/nexarr/platform-admins`, `/nexarr/security-audit` |
| StaffArr | `/staffarr/people`, `/staffarr/org`, `/staffarr/locations`, `/staffarr/positions`, `/staffarr/teams`, `/staffarr/roles`, `/staffarr/permissions`, `/staffarr/incidents`, `/staffarr/me` |
| TrainArr | `/trainarr/programs`, `/trainarr/catalog`, `/trainarr/assignments`, `/trainarr/learning`, `/trainarr/evaluations`, `/trainarr/certificates`, `/trainarr/qualifications`, `/trainarr/remediation`, `/trainarr/renewals` |
| MaintainArr | `/maintainarr/assets`, `/maintainarr/reservations`, `/maintainarr/work-orders`, `/maintainarr/defects`, `/maintainarr/inspections`, `/maintainarr/inspection-templates`, `/maintainarr/preventive-maintenance`, `/maintainarr/readiness`, `/maintainarr/parts` |
| SupplyArr | `/supplyarr/suppliers`, `/supplyarr/items`, `/supplyarr/sourcing`, `/supplyarr/purchase-requests`, `/supplyarr/purchase-orders`, `/supplyarr/contracts`, `/supplyarr/supplier-portal`, `/supplyarr/performance` |
| LoadArr | `/loadarr/inbound`, `/loadarr/dock-schedule`, `/loadarr/receiving`, `/loadarr/putaway`, `/loadarr/inventory`, `/loadarr/reservations`, `/loadarr/picking`, `/loadarr/staging`, `/loadarr/shipping`, `/loadarr/transfers`, `/loadarr/counts`, `/loadarr/adjustments`, `/loadarr/exceptions` |
| AssurArr | `/assurarr/nonconformances`, `/assurarr/holds`, `/assurarr/capa`, `/assurarr/audits`, `/assurarr/findings`, `/assurarr/complaints`, `/assurarr/inspections`, `/assurarr/supplier-quality`, `/assurarr/scorecards` |
| OrdArr | `/ordarr/orders`, `/ordarr/requests`, `/ordarr/triage`, `/ordarr/handoffs`, `/ordarr/holds`, `/ordarr/returns`, `/ordarr/exceptions`, `/ordarr/completion-packets`, `/ordarr/financial-packets` |
| LedgArr | `/ledgarr/dashboard`, `/ledgarr/legal-entities`, `/ledgarr/fiscal-periods`, `/ledgarr/chart-of-accounts`, `/ledgarr/dimensions`, `/ledgarr/financial-packets`, `/ledgarr/posting-preview`, `/ledgarr/journals`, `/ledgarr/ap`, `/ledgarr/ar`, `/ledgarr/inventory-valuation`, `/ledgarr/fixed-assets`, `/ledgarr/projects`, `/ledgarr/budgets`, `/ledgarr/tax`, `/ledgarr/reconciliation`, `/ledgarr/external`, `/ledgarr/reports` |
| CustomArr | `/customarr/dashboard`, `/customarr/customers`, `/customarr/locations`, `/customarr/contacts`, `/customarr/leads`, `/customarr/opportunities`, `/customarr/proposals`, `/customarr/agreements`, `/customarr/cases`, `/customarr/activities`, `/customarr/tasks`, `/customarr/portal-access`, `/customarr/requirements`, `/customarr/onboarding`, `/customarr/health`, `/customarr/imports`, `/customarr/merge-review`, `/customarr/settings` |
| RoutArr | `/routarr/dispatch`, `/routarr/demands`, `/routarr/planning`, `/routarr/tenders`, `/routarr/routes`, `/routarr/trips`, `/routarr/stops`, `/routarr/visibility`, `/routarr/yard`, `/routarr/appointments`, `/routarr/rating`, `/routarr/claims`, `/routarr/exceptions`, `/routarr/readiness` |
| RecordArr | `/recordarr/records`, `/recordarr/capture`, `/recordarr/imports`, `/recordarr/classification`, `/recordarr/packages`, `/recordarr/evidence-mapping`, `/recordarr/controlled-documents`, `/recordarr/retention`, `/recordarr/legal-holds`, `/recordarr/shares`, `/recordarr/access-history` |
| ReportArr | `/reportarr/builder`, `/reportarr/datasets`, `/reportarr/dashboards`, `/reportarr/reports`, `/reportarr/runs`, `/reportarr/schedules`, `/reportarr/kpis`, `/reportarr/alerts`, `/reportarr/distribution`, `/reportarr/provenance` |
| Field Companion | `/field/tasks`, `/field/capture`, `/field/inspections`, `/field/work-orders`, `/field/training`, `/field/warehouse`, `/field/routes`, `/field/uploads`, `/field/offline-queue`, `/field/sync`, `/field/context/:sourceProduct/:sourceObjectType/:sourceObjectId` |
| STL Compliance Site | `/site/pages`, `/site/products`, `/site/industries`, `/site/resources`, `/site/leads`, `/site/contact`, `/site/legal`, `/site/trust-status` |

## Compliance Core administrative studio

The administrative studio is platform-admin-only and uses these route groups:

`/compliancecore/catalogs`, `/compliancecore/vocabulary`, `/compliancecore/governing-bodies`, `/compliancecore/sources`, `/compliancecore/citations`, `/compliancecore/rulepacks`, `/compliancecore/requirements`, `/compliancecore/applicability`, `/compliancecore/evidence-mapping`, `/compliancecore/questionnaires`, `/compliancecore/facts`, `/compliancecore/evaluations`, `/compliancecore/tse`, `/compliancecore/imports`, `/compliancecore/review-queues`, `/compliancecore/publish`, `/compliancecore/audit`.

Compliance Core runtime operation is not a tenant-facing product route requirement. Ordinary products invoke runtime evaluation APIs and render the result within their own pages.

## Primary-record route contract

Every product-owned primary record normally provides:

```text
/{product}/{records}
/{product}/{records}/new
/{product}/{records}/{displayId}
/{product}/{records}/{displayId}/edit
/{product}/{records}/{displayId}/{domain-tab}
```

A preview drawer may supplement list navigation but does not replace the canonical detail page. Lifecycle actions use named server actions rather than arbitrary client-side status editing.

## Unified page contract

Every route declares its page archetype and implements the applicable page constitution. It must include designed loading, empty, no-results, forbidden, not-found, stale, degraded, partial, conflict, and error behavior as relevant. It must use shared semantic tokens and components in light and dark modes.

## Blocked-state standard

Blocked views identify the blocked object, reason, owner product, clearing action, responsible role/person, evidence required, override policy, and last evaluation time. Cross-product blockers link to an owner-backed view or delegated action.

## Print and report standard

Print routes render a professional report document without the application shell. They include tenant/context, subject, filters, generation timestamp, source/freshness, and page-safe typography rather than printing the interactive page verbatim.
