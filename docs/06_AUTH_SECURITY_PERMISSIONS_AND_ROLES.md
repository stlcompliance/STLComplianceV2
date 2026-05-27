# Auth, Security, Permissions, and Roles

## Identity Model

- NexArr owns login, tenant access, entitlement, and service identity.
- StaffArr owns the person record, org assignment, permissions, certifications, and readiness.
- A person can exist without login capability.
- Products reference persons by stable IDs and local references.

## Browser Flow

1. User signs in through NexArr.
2. NexArr validates credentials, tenant access, and product entitlement context.
3. NexArr issues access and session-renewal tokens.
4. Suite Frontend calls product APIs with the user token.
5. Product APIs validate token, tenant, entitlement, and product permission.

## Service Flow

1. Product service uses NexArr-governed service identity.
2. Product service presents a service token.
3. Receiving API validates service identity, tenant scope, product scope, and action scope.
4. Every service call carries correlation ID.

## Permission Key Format

`{product}.{domain}.{action}`

Examples:

- `staffarr.people.read`
- `maintainarr.workorders.close`
- `routarr.dispatch.assign`
- `supplyarr.purchaseorders.approve`
- `trainarr.programs.publish`
- `compliancecore.rulepacks.publish`

## Default Roles

| Role | Primary Scope |
|---|---|
| Platform Owner | Full NexArr and break-glass review |
| Platform Admin | Tenants, products, entitlements, service clients, audit |
| Tenant Admin | Tenant bootstrap, product access support, people bootstrap |
| Workforce Admin | StaffArr people, org, permissions, certifications, readiness |
| Training Admin | TrainArr programs, requirements, assignments |
| Trainer / Evaluator | TrainArr evaluation and signoff |
| Maintenance Manager | MaintainArr assets, inspections, WO, PM, readiness |
| Technician | MaintainArr assigned work and inspection execution |
| Dispatcher | RoutArr route/trip/dispatch operations |
| Driver | RoutArr assigned trip and DVIR execution |
| Supply Manager | SupplyArr vendors, inventory, PR/PO, receiving |
| Buyer | SupplyArr purchasing workflow |
| Compliance Admin | Compliance Core vocabulary, keys, rule packs, mappings |
| Compliance Reviewer | Findings, reports, audit packages |
| Read-Only Auditor | Read/report/export where permitted |

## Sensitive Actions

Sensitive actions require explicit permission and audit logging:

- Grant entitlement
- Change tenant state
- Create service token
- Assign platform admin
- Assign permissions
- Override readiness
- Publish qualifications
- Close high-risk work orders
- Dispatch with blocked readiness
- Approve purchase orders
- Publish rule packs
- Export audit packages

## Logging Rules

Do not log raw passwords, full tokens, signing keys, database URLs, or service secrets. Log safe actor, tenant, product, action, target, correlation ID, result, and reason code.
