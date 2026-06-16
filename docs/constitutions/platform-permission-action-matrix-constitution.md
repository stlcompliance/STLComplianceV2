# STL Compliance Permission and Action Matrix Constitution

## 1. Purpose

This constitution defines how product permissions, role defaults, risky actions, approval gates, and UI action visibility should be documented across STL Compliance.

StaffArr owns central permission assignment context.

Products own domain authorization checks for their own actions.

NexArr owns platform login, platform admin, tenant entitlement, launch, service-client, and service-token authority.

## 2. Scope

This constitution applies to:

- Product permission keys
- Role defaults
- UI action visibility
- API authorization
- Risky action gates
- Override permissions
- External portal action limits
- Field Companion action limits
- Audit requirements for sensitive actions

## 3. Prime directive

A visible button is not authorization.

Every sensitive product action must be checked server-side by the owning product using StaffArr permission/authority context and NexArr identity/entitlement context.

## 4. Permission key format

Permissions must use canonical product key prefixes:

```text
{productKey}.{domain}.{action}
```

Examples:

```text
maintainarr.work_orders.create
maintainarr.assets.update_readiness
loadarr.inventory.post_adjustment
routarr.dispatch.release
supplyarr.purchase_orders.approve
trainarr.certificates.issue
staffarr.permissions.assign
nexarr.platform_admin.manage
recordarr.records.download_sensitive
compliancecore.rulepacks.publish
assurarr.holds.release
ordarr.orders.close
customarr.customers.approve
```

## 5. Permission matrix row

Each product should maintain a permission/action matrix.

```text
PermissionActionMatrixRow
- permissionKey
- displayName
- description
- productKey
- domain
- action
- actionCategory
  - view
  - create
  - edit
  - approve
  - close
  - release
  - override
  - delete
  - export
  - administer
  - integration
- riskLevel
  - low
  - medium
  - high
  - critical
- defaultRoles
- requiresApproval
- requiresReason
- requiresEvidence
- requiresQualification
- requiresMfaOrReauth
- auditLevel
  - standard
  - sensitive
  - security
  - compliance
- fieldCompanionAllowed
- externalPortalAllowed
- apiOnly
- notes
```

## 6. Risk levels

### Low

Read or routine capture actions with limited harm.

Examples:

```text
- view assigned work
- acknowledge task
- upload non-sensitive photo
```

### Medium

Operational changes that affect workflow but are reversible.

Examples:

```text
- create work order
- create order request
- stage received items
```

### High

Actions that affect readiness, inventory, customer commitments, supplier status, dispatch, certificates, evidence, or compliance posture.

Examples:

```text
- update asset readiness
- post inventory adjustment
- dispatch route
- issue certificate
- approve supplier
- close order
```

### Critical

Actions that override controls, release holds, publish rulepacks, manage platform admin, delete/supersede records, or export sensitive information.

Examples:

```text
- override blocker
- release quality hold
- publish Compliance Core rulepack
- assign platform admin
- revoke service token
- delete/supersede controlled record
```

## 7. Default role guidance

Products may define default role bundles, but permission assignment truth remains StaffArr.

Recommended role categories:

```text
- viewer
- operator
- technician
- supervisor
- manager
- reviewer
- approver
- admin
- platform_admin where NexArr-owned
```

Do not hardcode role names as authorization logic if permission checks are available.

## 8. Product evaluation

Owning products must evaluate permissions at API boundary.

The API should check:

```text
- authenticated identity
- tenant membership
- product entitlement
- StaffArr authority context
- product permission key
- record-level restrictions
- site/location scope
- workflow state
- blocker/approval requirements
```

## 9. Record-level scope

A permission may still be limited by:

```text
- site
- location
- department
- team
- assigned person
- customer
- supplier
- route
- order type
- record sensitivity
- external portal invitation scope
```

## 10. UI behavior

UI should:

```text
- hide actions clearly outside permission scope
- disable actions blocked by workflow state with explanation
- show required approval or reason before submission
- show external portal restrictions
- never rely on UI-only enforcement
```

## 11. Sensitive action audit

Sensitive actions should record:

```text
- actor personId or service client
- permission key used
- tenantId
- productKey
- target record
- previous state
- new state
- reason
- evidence refs where required
- correlationId
- timestamp
```

## 12. External portal limits

External portal access must use limited action scopes, not internal product role bundles.

Example:

```text
Allowed:
- supplyarr.external_vendor.confirm_completion

Not allowed:
- supplyarr.purchase_orders.manage
```

## 13. Field Companion limits

Field Companion may expose product actions, but the owning product still authorizes them.

Offline actions must be validated when synced.

If permission or readiness state changed while offline, the owning product may reject or require review.

## 14. Permission documentation requirement

Each product should have either:

```text
- a local product permission matrix document
```

or

```text
- a row set in a shared platform permission/action matrix
```

The matrix should be kept close to implementation so API, UI, docs, and tests do not drift.

## 15. Non-goals

This constitution does not make StaffArr responsible for every product authorization decision.

StaffArr stores assignment and authority context. The product that owns the action enforces the domain rule.
