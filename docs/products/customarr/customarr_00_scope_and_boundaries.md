# CustomArr — Scope, Ownership, and Boundaries

## Product purpose

CustomArr is the customer master and customer relationship control plane for tenant-owned business customers.

CustomArr is the source of truth for customers of tenants. These are not STL Compliance platform tenants and not internal employees. They are the external organizations, accounts, locations, contacts, consignees, shippers, bill-to parties, ship-to parties, service recipients, and customer-specific requirements that tenant operations reference across the suite.

CustomArr answers:

- Who are this tenant's customers?
- Which customer account is the canonical account?
- Which customer names, aliases, external IDs, and legacy references resolve to the same account?
- Which customer sites, bill-to locations, ship-to locations, pickup locations, dropoff locations, and service locations exist?
- Which customer contacts exist and what are they allowed to approve, receive, sign, request, or view?
- What lifecycle state does this customer have: prospect, onboarding, active, inactive, or archived?
- Is this customer/location eligible, limited, blocked, pending review, or unknown for order creation, dispatch, delivery, service, release, or other product workflows?
- What customer-specific requirements must be satisfied before work proceeds?
- What documents, contracts, requirements, preferences, holds, and exceptions are attached to the customer relationship?
- Which owning product should enforce a requirement or resolve a blocker?

## CustomArr owns

```text
- Tenant customer master
- Customer accounts
- Customer account hierarchy
- Customer groups
- Customer aliases
- Customer external system mappings
- Customer account status
- Customer onboarding status
- Customer service eligibility snapshot
- Customer operational hold status
- Customer contact master
- Customer contact authorization scope
- Customer communication preferences
- Customer portal contact linkage references
- Customer external locations
- Customer bill-to / ship-to / pickup / dropoff / service location identity
- Customer location status
- Customer location hours and access instructions
- Customer-specific operational requirements
- Customer-specific contractual requirement references
- Customer-specific safety / quality / documentation requirements
- Customer-specific service restrictions
- Customer-specific preferences
- Customer contract references and summaries
- Customer requirement waiver records
- Customer approval records
- Customer relationship risk snapshot
- Customer relationship notes and communications
- Customer duplicate detection and merge history
- Customer-origin events
- Customer audit trail
```

## CustomArr does not own

```text
- Platform tenant identity
- Platform login
- Tenant entitlement
- Internal person master
- Internal permission assignment truth
- Internal StaffArr location identity
- Employee training/certification truth
- Supplier/vendor master
- Procurement truth
- Purchase requests
- Purchase orders
- Item/product master
- Inventory balance
- Stock ledger
- Receiving
- Putaway
- Pick/issue/ship execution truth
- Customer order lifecycle
- Dispatch/route/trip execution
- Maintenance execution
- Asset readiness
- Quality hold/release truth
- Regulatory/rulepack meaning
- Actual document/file/evidence object
- Reporting read models
- Accounting execution
- General ledger
- Accounts receivable ledger
- Invoices
- Payments
- Tax calculation
- Credit ledger
- Sales opportunity pipeline unless explicitly added later
```

## Important naming boundary

```text
CustomArr customers
- External customers belonging to a tenant.
- Examples: a pallet broker's customers, a shipper, consignee, bill-to account, delivery recipient, service customer, facility customer, or contracted business account.

NexArr tenants
- STL Compliance platform tenants.
- Examples: tenant organizations that subscribe to STL Compliance products.

StaffArr people
- Internal people/person records used by the tenant.
- Examples: employees, technicians, managers, operators, dispatchers, warehouse users.

CustomArr contacts
- External contacts at customer accounts and customer locations.
- Examples: receiver, buyer, customer operations contact, customer compliance contact, billing contact, emergency contact.
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens
- Customer portal identity linkage when external customer contacts can log in

StaffArr
- Internal person references
- Internal account owner references
- Internal sales / operations owner references
- Internal approver references
- Product permission assignments
- Internal site/location references only when assigning responsibility

TrainArr
- Training/qualification status for tenant people who must satisfy customer-specific requirements
- Customer-required training assignment requests
- Remediation requests caused by customer incidents or failed requirements

Compliance Core
- Governing body catalogs
- Rulepacks
- Regulatory evaluations
- Regulatory meaning for customer-specific compliance requirements
- Evidence requirement definitions
- Requirement evaluation support when requirements have regulatory meaning

RecordArr
- Customer contracts
- Customer onboarding documents
- Insurance certificates
- Tax exemption documents
- Customer policies
- Customer correspondence records
- Customer requirement evidence
- Customer approval evidence
- Uploaded customer files

OrdArr
- Customer order lifecycle
- Customer order creation checks
- Customer order status facts
- Bill-to / ship-to / sold-to / consignee references

RoutArr
- Route/trip execution
- Pickup/dropoff customer location references
- Appointment and delivery exception facts
- Route customer impact facts

LoadArr
- Fulfillment / warehouse execution
- Ship-to customer location references
- Dock / staging / load facts tied to customer orders
- Shipment readiness facts where customer requirements affect release

MaintainArr
- Customer-owned asset references when tenant services customer assets
- Customer impact facts on work orders, defects, downtime, and service readiness

SupplyArr
- Supplier/vendor master
- Supplier linkage when the same legal entity is both customer and supplier
- Sourcing facts only when needed for customer-specific sourcing requirements

AssurArr
- Quality holds
- Nonconformance
- CAPA
- Customer complaint facts
- Quality release decisions

ReportArr
- Customer dashboards
- Customer KPIs
- Cross-product reporting views

Field Companion
- Mobile customer site check-in
- Customer signatures
- Customer contact confirmation
- Photo/evidence capture at customer locations
- Mobile execution of customer requirement prompts
```

## Core source-of-truth rules

```text
1. CustomArr owns tenant customer/account identity.
2. CustomArr owns external customer location identity.
3. CustomArr owns external customer contact identity.
4. CustomArr owns customer account status, customer location status, and customer relationship status.
5. CustomArr owns customer-specific requirements, preferences, restrictions, and operational hold records.
6. NexArr owns STL Compliance tenant identity and login.
7. StaffArr owns internal people and internal locations.
8. CustomArr must not create canonical StaffArr internal locations.
9. CustomArr customer contacts are not StaffArr people unless explicitly linked through a supported external-identity pattern.
10. OrdArr owns customer order lifecycle and references CustomArr customers, locations, and contacts.
11. RoutArr owns route/trip execution and references CustomArr pickup/dropoff/customer locations.
12. LoadArr owns warehouse execution and references CustomArr ship-to / consignee / customer locations when required.
13. SupplyArr owns supplier/vendor truth. If the same legal entity is both a customer and a supplier, CustomArr and SupplyArr records are linked, not merged.
14. MaintainArr owns maintenance execution and references CustomArr only for customer-owned assets or customer impact.
15. AssurArr owns quality hold/release decisions. CustomArr may surface customer-facing hold status snapshots but does not decide quality release.
16. Compliance Core owns regulatory meaning. CustomArr owns the customer requirement record and links to Compliance Core when regulatory interpretation is needed.
17. RecordArr owns actual document/file/evidence objects. CustomArr stores references, summaries, metadata, and status snapshots.
18. Accounting systems own invoices, payments, ledger, credit, and tax calculation. CustomArr may store customer account references and operational hold snapshots from accounting.
19. ReportArr owns reporting views, not customer truth.
20. No product should store free-text customer identity when a CustomArr customerRef can be used.
```

## Standard CustomArr object envelope

Every major CustomArr object should include:

```text
CustomArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- sourceProduct
- sourceObjectRef
- customerRef
- customerLocationRef
- customerContactRef
- recordRefs
- complianceRefs
- auditTrail
- eventLog
```

## Standard structured reference

```text
SuiteRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard customer reference

```text
CustomerRef
- customerId
- customerNumber
- displayNameSnapshot
- legalNameSnapshot
- statusSnapshot
- serviceEligibilitySnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard customer location reference

```text
CustomerLocationRef
- customerLocationId
- customerId
- locationNumber
- displayNameSnapshot
- locationTypeSnapshot
- addressSnapshot
- statusSnapshot
- serviceEligibilitySnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard customer contact reference

```text
CustomerContactRef
- customerContactId
- customerId
- displayNameSnapshot
- titleSnapshot
- contactTypeSnapshot
- authorizationScopeSnapshot
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## CustomArr object prefixes

```text
CUS     Customer account
CGRP    Customer group
CAL     Customer alias
CEXT    Customer external system mapping
CLOC    Customer location
CCON    Customer contact
CMET    Customer contact method
CHRS    Customer location hours
CACC    Customer access requirement
CREQ    Customer requirement
CEVL    Customer requirement evaluation
CWAV    Customer requirement waiver
CPRF    Customer preference
CSVC    Customer service profile
CHLD    Customer hold
CCTR    Customer contract reference
CONB    Customer onboarding
CAPR    Customer approval
CRSK    Customer risk profile
CCOM    Customer communication log
CEXC    Customer exception
CMRG    Customer merge record
CINV    Customer portal invite
CPAX    Customer portal access record
```
