# STL Compliance V2 Product UI Route Map

## Purpose

This document maps product models into practical UI route groups.

The route map is intentionally product-owned. A route may display cross-product context, but source edits happen in the owning product.

## Shared route rules

Every product should expose:

```text
/{product}/
/{product}/search
/{product}/tasks
/{product}/reports where product-local
/{product}/settings where product-local
/{product}/audit where authorized
```

Shared components should include:

```text
- list/table views
- boards/queues
- create wizard
- detail header
- status badges
- blocker panels
- evidence drawer
- activity feed
- audit trail summary
- cross-product references
- external portal submission summary
- AI proposal review panel
```

## Platform shell

```text
/
  product launcher
  task inbox
  notifications
  global search where authorized
  profile/session
  tenant switch where authorized
  product entitlement state

/admin
  NexArr-owned platform admin routes only
```

## Product route groups

| Product | Primary routes |
|---|---|
| NexArr | `/nexarr/tenants`, `/nexarr/entitlements`, `/nexarr/product-launch`, `/nexarr/service-clients`, `/nexarr/platform-admins`, `/nexarr/audit` |
| StaffArr | `/staffarr/people`, `/staffarr/org`, `/staffarr/locations`, `/staffarr/roles`, `/staffarr/permissions`, `/staffarr/teams`, `/staffarr/incidents`, `/staffarr/me` |
| TrainArr | `/trainarr/programs`, `/trainarr/assignments`, `/trainarr/evaluations`, `/trainarr/certificates`, `/trainarr/qualifications`, `/trainarr/remediation`, `/trainarr/renewals` |
| MaintainArr | `/maintainarr/assets`, `/maintainarr/work-orders`, `/maintainarr/defects`, `/maintainarr/inspections`, `/maintainarr/inspection-templates`, `/maintainarr/preventive-maintenance`, `/maintainarr/readiness` |
| SupplyArr | `/supplyarr/suppliers`, `/supplyarr/vendors`, `/supplyarr/items`, `/supplyarr/purchase-requests`, `/supplyarr/purchase-orders`, `/supplyarr/sourcing`, `/supplyarr/performance` |
| LoadArr | `/loadarr/items`, `/loadarr/locations`, `/loadarr/balances`, `/loadarr/receiving`, `/loadarr/putaway`, `/loadarr/reservations`, `/loadarr/picks`, `/loadarr/issues`, `/loadarr/transfers`, `/loadarr/counts`, `/loadarr/adjustments` |
| AssurArr | `/assurarr/nonconformances`, `/assurarr/holds`, `/assurarr/capa`, `/assurarr/audit-findings`, `/assurarr/complaints`, `/assurarr/scorecards` |
| OrdArr | `/ordarr/orders`, `/ordarr/requests`, `/ordarr/triage`, `/ordarr/handoffs`, `/ordarr/exceptions`, `/ordarr/completion-packets`, `/ordarr/financial-packets` |
| LedgArr | `/ledgarr/dashboard`, `/ledgarr/financial-legal-entities`, `/ledgarr/fiscal-periods`, `/ledgarr/chart-of-accounts`, `/ledgarr/dimensions`, `/ledgarr/financial-packets`, `/ledgarr/posting-preview`, `/ledgarr/journals`, `/ledgarr/ap`, `/ledgarr/ar`, `/ledgarr/inventory-valuation`, `/ledgarr/fixed-assets`, `/ledgarr/projects`, `/ledgarr/budgets`, `/ledgarr/tax`, `/ledgarr/external`, `/ledgarr/reports` |
| CustomArr | `/customarr/dashboard`, `/customarr/accounts`, `/customarr/customers`, `/customarr/locations`, `/customarr/contacts`, `/customarr/leads`, `/customarr/opportunities`, `/customarr/proposals`, `/customarr/agreements`, `/customarr/cases`, `/customarr/activities`, `/customarr/tasks`, `/customarr/portal-access`, `/customarr/requirements`, `/customarr/eligibility`, `/customarr/onboarding`, `/customarr/health`, `/customarr/imports`, `/customarr/merge-review`, `/customarr/integration-references`, `/customarr/settings` |
| RoutArr | `/routarr/dispatch`, `/routarr/transportation-demands`, `/routarr/routes`, `/routarr/trips`, `/routarr/stops`, `/routarr/proofs`, `/routarr/exceptions`, `/routarr/driver-equipment-readiness`, `/routarr/dock-appointments`, `/routarr/load-visibility` |
| RecordArr | `/recordarr/records`, `/recordarr/upload`, `/recordarr/packages`, `/recordarr/evidence-mapping`, `/recordarr/controlled-documents`, `/recordarr/retention`, `/recordarr/legal-holds`, `/recordarr/access-history` |
| Compliance Core | `/compliancecore/catalogs`, `/compliancecore/governing-bodies`, `/compliancecore/citations`, `/compliancecore/rulepacks`, `/compliancecore/requirements`, `/compliancecore/applicability`, `/compliancecore/evidence-requirements`, `/compliancecore/questionnaires`, `/compliancecore/facts`, `/compliancecore/tse`, `/compliancecore/imports` |
| ReferenceDataCore | `/referencedatacore/datasets`, `/referencedatacore/imports`, `/referencedatacore/staging`, `/referencedatacore/entities`, `/referencedatacore/crosswalks`, `/referencedatacore/aliases`, `/referencedatacore/uom`, `/referencedatacore/package-rules`, `/referencedatacore/publish-history`, `/referencedatacore/conflicts` |
| ReportArr | `/reportarr/datasets`, `/reportarr/dashboards`, `/reportarr/widgets`, `/reportarr/reports`, `/reportarr/runs`, `/reportarr/schedules`, `/reportarr/kpis`, `/reportarr/provenance` |
| Field Companion | `/field/tasks`, `/field/capture`, `/field/inspections`, `/field/work-orders`, `/field/training`, `/field/uploads`, `/field/offline-queue`, `/field/sync`, `/field/context/:sourceProduct/:sourceObjectType/:sourceObjectId` |
| STL Compliance Site | `/site/pages`, `/site/products`, `/site/industries`, `/site/leads`, `/site/contact`, `/site/legal`, `/site/trust-status` |

## Blocked-state UI standard

Every blocked state should show:

```text
- blocked object
- blocker reason
- owning product
- clearing action
- who can clear it
- whether override is allowed
- evidence needed
- last checked time
```

## Create-flow standard

Create flows should prefer:

```text
1. required fields first
2. hydrated/selectable data from owning products
3. validation before final submit
4. review page before irreversible actions
5. no raw JSON
6. save draft when appropriate
```

## Product detail standard

Every product detail screen should show:

```text
- title and stable display number
- status and lifecycle category
- ownership/source-of-truth badge
- related cross-product references
- blockers and warnings
- evidence drawer
- activity feed
- audit summary where authorized
- actions filtered by permission and workflow state
```

## Compliance Core access note

Compliance Core management routes require server-side platform-admin validation unless a route is specifically implemented as a product-consumption service-token API.

## Field Companion note

Field Companion does not own source records.

It is a mobile execution surface that calls owning product APIs and syncs offline captures for review/acceptance by the owning product.
