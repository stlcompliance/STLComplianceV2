# Permission Keys and Default Roles

## Permission Naming

Format: `{product}.{domain}.{action}`

Common actions: access, read, create, edit, delete, manage, assign, approve, reject, close, publish, export, override, administer.

## Product Access Keys

- nexarr.access
- staffarr.access
- trainarr.access
- maintainarr.access
- routarr.access
- supplyarr.access
- compliancecore.access

## Permission Catalog

### NexArr

- nexarr.tenants.read
- nexarr.tenants.create
- nexarr.tenants.edit
- nexarr.tenants.suspend
- nexarr.products.manage
- nexarr.entitlements.grant
- nexarr.entitlements.revoke
- nexarr.serviceclients.manage
- nexarr.platform.administer
- nexarr.audit.export

### StaffArr

- staffarr.people.read
- staffarr.people.create
- staffarr.people.edit
- staffarr.org.manage
- staffarr.permissions.assign
- staffarr.certifications.manage
- staffarr.readiness.override
- staffarr.incidents.manage
- staffarr.notes.manage
- staffarr.documents.manage
- staffarr.audit.export

### TrainArr

- trainarr.programs.create
- trainarr.programs.publish
- trainarr.requirements.manage
- trainarr.assignments.create
- trainarr.assignments.complete
- trainarr.evidence.upload
- trainarr.evaluations.signoff
- trainarr.qualifications.issue
- trainarr.audit.export

### MaintainArr

- maintainarr.assets.create
- maintainarr.inspections.perform
- maintainarr.inspections.manage
- maintainarr.defects.create
- maintainarr.workorders.create
- maintainarr.workorders.perform
- maintainarr.workorders.close
- maintainarr.pm.manage
- maintainarr.readiness.override
- maintainarr.audit.export

### RoutArr

- routarr.routes.create
- routarr.dispatch.assign
- routarr.dispatch.manage
- routarr.trips.perform
- routarr.dvir.perform
- routarr.exceptions.create
- routarr.exceptions.manage
- routarr.audit.export

### SupplyArr

- supplyarr.vendors.manage
- supplyarr.parts.manage
- supplyarr.inventory.manage
- supplyarr.purchaseRequests.create
- supplyarr.purchaseRequests.approve
- supplyarr.purchaseOrders.create
- supplyarr.purchaseOrders.approve
- supplyarr.receiving.perform
- supplyarr.audit.export

### Compliance Core

- compliancecore.vocabulary.manage
- compliancecore.keys.manage
- compliancecore.rulepacks.create
- compliancecore.rulepacks.publish
- compliancecore.mappings.manage
- compliancecore.sds.manage
- compliancecore.findings.manage
- compliancecore.audit.export
- compliancecore.administer

## Default Roles

| Role | Primary Product | Purpose |
|---|---|---|
| Platform Owner | NexArr | Full platform control and break-glass review |
| Platform Admin | NexArr | Tenant, product, entitlement, service client, audit control |
| Tenant Admin | Suite | Tenant bootstrap and product access support |
| Workforce Admin | StaffArr | People, org, permissions, certifications, readiness |
| Training Admin | TrainArr | Programs, requirements, assignments |
| Trainer / Evaluator | TrainArr | Evaluation and signoff |
| Maintenance Manager | MaintainArr | Assets, inspections, WO, PM, readiness |
| Technician | MaintainArr | Assigned work and inspection execution |
| Dispatcher | RoutArr | Routes, trips, assignments, exceptions |
| Driver | RoutArr | Assigned trips, DVIR, proof, exception reporting |
| Supply Manager | SupplyArr | Vendors, inventory, purchasing, receiving |
| Buyer | SupplyArr | Purchase request/order workflow |
| Compliance Admin | Compliance Core | Vocabulary, keys, rule packs, mappings |
| Compliance Reviewer | Compliance Core | Findings, reports, audit packages |
| Read-Only Auditor | Suite | Read/report/export where permitted |
