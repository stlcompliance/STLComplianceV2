# CustomArr User Guide

## What This Product Is For
CustomArr is for customer accounts, locations, contacts, leads, opportunities, proposals, agreements, customer cases, activities, tasks, portal access records, service profiles, eligibility checks, customer requirements, onboarding, health/success snapshots, imports, duplicate review, merge review, integration references, communication preferences, and customer risk or review context.

CustomArr owns customer relationship and commercial intent truth. It does not own public-site marketing pages, order orchestration, dispatch execution, warehouse execution, maintenance execution, retained files, platform identity, regulatory interpretation, or accounting/ledger truth.

## Who Uses It
- customer operations users
- account managers
- onboarding reviewers
- customer service users
- compliance users reviewing customer-specific requirements

## Main Pages
- Dashboard
- Accounts
- Pipeline
- Commercial
- Support
- Operations
- Health
- Imports & Merge
- Integrations
- Settings

## Main Records
- customer account
- customer location
- customer contact
- lead
- opportunity
- proposal snapshot
- agreement metadata/reference
- customer case
- activity event
- task
- portal access record
- service profile
- eligibility check
- customer onboarding
- customer contact authorization
- customer access requirement
- customer requirement
- customer communication preference
- customer health profile
- import batch
- dedupe candidate
- merge review record
- integration reference

## Common Workflows
- create a customer prospect or onboarding record
- create and convert a lead
- create and advance an opportunity
- mark an opportunity won to request a downstream handoff
- create a proposal snapshot and record customer acceptance
- approve onboarding and activate a customer account
- create a customer case and assign follow-up tasks
- add customer contacts and authorization records
- add customer locations and access instructions
- manage service eligibility separately from account lifecycle
- run an eligibility check before portal order forwarding
- grant, suspend, or revoke customer portal access
- stage imports, review duplicates, and propose merges
- manage external mappings and integration references

## Permissions Usually Needed
- customarr.accounts.read
- customarr.accounts.manage
- customarr.leads.read
- customarr.leads.manage
- customarr.leads.convert
- customarr.opportunities.read
- customarr.opportunities.manage
- customarr.opportunities.handoff
- customarr.proposals.read
- customarr.proposals.manage
- customarr.proposals.accept
- customarr.cases.read
- customarr.cases.manage
- customarr.contacts.manage
- customarr.locations.manage
- customarr.eligibility.check
- customarr.portal_access.manage
- customarr.imports.read
- customarr.imports.manage
- customarr.integration_references.manage

## Related Products
- OrdArr owns order and request orchestration for customers.
- RoutArr, LoadArr, MaintainArr, and other execution products own execution truth.
- RecordArr stores retained files and persistent evidence links.
- NexArr owns platform identity, product launch, entitlement, and portal trust.
- Compliance Core owns regulatory meaning for requirements.
- StaffArr owns internal people, teams, role assignments, and internal locations referenced by owner fields.
- Finance systems own invoices, payments, tax, ledger, and accounting close.
- Accepted CustomArr opportunities and proposals create explicit handoff requests or refs rather than directly creating execution records.

## Common Troubleshooting
- [Product not visible](../troubleshooting/product-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If an order, trip, work order, or receiving task is missing, check the owning execution product or OrdArr rather than editing the customer account.
- Remember: account lifecycle, onboarding status, and service eligibility are separate signals.
