# Data Ownership and Database Design

## Product Databases

- nexarr-db
- staffarr-db
- trainarr-db
- maintainarr-db
- routarr-db
- supplyarr-db
- compliancecore-db

## Rules

- One database per product.
- No cross-product foreign keys.
- No direct cross-product database writes.
- Products use APIs, events, service tokens, and local references.
- External data snapshots carry source product, source ID, source event, and source timestamp.
- Snapshots help display and audit; they do not transfer ownership.

## Local Reference Example

```txt
maintainarr_staff_person_refs
- id
- tenant_id
- staffarr_person_id
- display_name_snapshot
- active_status_snapshot
- primary_site_snapshot
- last_seen_at
- source_correlation_id
```

## Tenant Columns

Tenant-scoped tables include:

- tenant_id
- created_at
- created_by where applicable
- modified_at where applicable
- modified_by where applicable
- status where applicable

## Audit Minimum

- id
- tenant_id
- product
- actor_type
- actor_id
- action
- target_type
- target_id
- occurred_at
- correlation_id
- causation_id
- reason_code where useful
- before/after snapshots where safe

## Database Contents

| Database | Contains |
|---|---|
| nexarr-db | tenants, identities, credentials, session-renewal tokens, products, entitlements, subscriptions, service clients, service tokens, audit |
| staffarr-db | people, org, sites, departments, teams, roles, permissions, certifications, readiness, incidents, history |
| trainarr-db | programs, versions, requirements, assignments, steps, evidence, signoffs, evaluations, completions, qualifications |
| maintainarr-db | assets, classes, inspections, defects, work orders, PM, maintenance history, readiness, references |
| routarr-db | routes, trips, stops, dispatch, drivers refs, vehicle refs, DVIR, proof, exceptions, route history |
| supplyarr-db | vendors, suppliers, parts, catalogs, inventory, purchase requests, purchase orders, receiving, pricing, lead times |
| compliancecore-db | vocabulary, keys, material keys, mappings, rule packs, SDS/HazCom, findings, publication history |

## Data Plane Readiness

NexArr can remain the hosted control plane while product data can later live in hosted, customer-hosted, or hybrid services. Customer-hosted data remains untrusted until validated by the owning service.
