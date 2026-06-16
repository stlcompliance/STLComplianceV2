# NexArr — Tenant, Product, and Entitlement Model

## Tenant

A Tenant is the platform customer/account boundary. It determines isolation, entitlement, data-plane behavior, product access, security policy, and administrative scope.

```text
Tenant
- tenantId
- tenantNumber
- legalName
- displayName
- slug
- status
  - prospect
  - trial
  - active
  - suspended
  - canceled
  - archived
- tenantType
  - internal
  - customer
  - demo
  - sandbox
  - partner
  - test
- primaryContactPersonId
- ownerPersonId
- defaultTimezone
- defaultLocale
- dataRegion
- deploymentMode
  - hosted
  - hybrid
  - customer_hosted_data_plane
- securityPolicyRef
- productEntitlementRefs
- billingAccountExternalRef
- supportTier
  - community
  - standard
  - priority
  - enterprise
  - custom
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- suspendedAt
- suspendedByPersonId
- suspensionReason
- canceledAt
- canceledByPersonId
- cancelReason
- archivedAt
- auditTrail
```

## Tenant status definitions

```text
prospect
- Tenant exists for sales/demo/onboarding context but has no production access.

trial
- Tenant can use entitled trial products within trial rules.

active
- Tenant can use entitled products normally.

suspended
- Tenant is blocked or limited because of billing, abuse, security, compliance, or administrative action.

canceled
- Tenant subscription relationship ended.

archived
- Tenant retained for history only.
```

## Tenant membership

A TenantMembership connects a person to a tenant.

```text
TenantMembership
- membershipId
- tenantId
- personId
- status
  - invited
  - active
  - suspended
  - removed
  - expired
- membershipType
  - employee
  - contractor
  - vendor
  - customer
  - auditor
  - service
  - admin
- invitedAt
- invitedByPersonId
- joinedAt
- suspendedAt
- suspendedByPersonId
- removedAt
- removedByPersonId
- expiresAt
- reason
- sourceProduct
- sourceObjectRef
```

## Tenant membership status definitions

```text
invited
- Person has been invited but has not accepted or activated access.

active
- Person belongs to tenant.

suspended
- Person remains linked but tenant access is temporarily blocked.

removed
- Person no longer belongs to tenant.

expired
- Membership expired automatically.
```

## Tenant setting

```text
TenantSetting
- settingId
- tenantId
- settingKey
- settingValue
- valueType
  - string
  - number
  - boolean
  - json
  - enum
- category
  - security
  - product
  - branding
  - data
  - notification
  - integration
- effectiveAt
- updatedByPersonId
- updatedAt
```

## Product registry entry

A ProductRegistryEntry defines a product known to the platform.

```text
ProductRegistryEntry
- productRegistryEntryId
- productKey
  - nexarr
  - staffarr
  - trainarr
  - compliancecore
  - maintainarr
  - loadarr
  - supplyarr
  - routarr
  - customarr
  - ordarr
  - recordarr
  - assurarr
  - reportarr
  - fieldcompanion
  - referencedatacore
  - stlcompliancesite
- displayName
- description
- productCategory
  - platform
  - people
  - training
  - compliance
  - maintenance
  - inventory
  - procurement
  - transportation
  - customer
  - orders
  - records
  - quality
  - reporting
  - mobile
  - reference_data
  - public_site
- status
  - planned
  - active
  - suspended
  - retired
- launchUrl
- apiBaseUrl
- healthCheckUrl
- iconKey
- requiredDependencies
- optionalDependencies
- defaultScopes
- createdAt
- updatedAt
```

## Product dependency rule

```text
ProductDependencyRule
- dependencyRuleId
- productKey
- requiredProductKey
- dependencyType
  - required
  - recommended
  - feature_dependency
  - integration_dependency
- reason
- enforced
- status
```

Examples:

```text
MaintainArr may require:
- NexArr
- StaffArr
- RecordArr
- Compliance Core optional/required depending tier
- LoadArr if parts inventory workflows are enabled

LoadArr may require:
- NexArr
- StaffArr
- RecordArr
- AssurArr optional/required for quality holds

Field Companion may require:
- NexArr
- StaffArr
- Source products enabled

ReferenceDataCore may require:
- NexArr
- StaffArr for admin authority context
```

## Product entitlement

ProductEntitlement is tenant-level product availability.

```text
ProductEntitlement
- entitlementId
- tenantId
- productKey
- status
  - trial
  - active
  - suspended
  - expired
  - canceled
  - archived
- tier
  - free
  - starter
  - professional
  - enterprise
  - custom
- startsAt
- endsAt
- trialEndsAt
- suspendedAt
- suspensionReason
- featureFlags
- seatLimit
- usageLimits
- dependencyStatus
- billingExternalRef
- grantedByPersonId
- grantedAt
- updatedAt
- lastCheckedAt
```

## Product entitlement status definitions

```text
trial
- Product is available under trial rules.

active
- Product is available normally.

suspended
- Product is temporarily blocked or limited.

expired
- Product entitlement ended by date.

canceled
- Product subscription ended intentionally.

archived
- Entitlement retained for history.
```

## Product feature flag

```text
ProductFeatureFlag
- featureFlagId
- tenantId
- productKey
- featureKey
- displayName
- status
  - enabled
  - disabled
  - preview
  - beta
  - deprecated
- tierRequirement
- enabledAt
- enabledByPersonId
- expiresAt
- metadata
```

## Product usage limit

```text
ProductUsageLimit
- usageLimitId
- tenantId
- productKey
- limitKey
- limitType
  - seats
  - records
  - storage
  - api_calls
  - exports
  - mobile_users
  - integrations
  - custom
- limitValue
- currentUsageSnapshot
- resetPeriod
  - none
  - daily
  - monthly
  - annual
- status
  - active
  - exceeded
  - warning
  - disabled
```

## Product access grant

ProductAccessGrant is person-level permission to launch/enter a product. It is not the same as product-domain permission.

```text
ProductAccessGrant
- grantId
- tenantId
- personId
- productKey
- status
  - pending
  - active
  - suspended
  - revoked
  - expired
- grantSource
  - direct
  - tenant_default
  - staffarr_role
  - staffarr_position
  - staffarr_team
  - temporary
  - system
- grantedByPersonId
- grantedAt
- expiresAt
- suspendedAt
- revokedAt
- reason
- staffarrPermissionSnapshot
```

## Product access decision

```text
ProductAccessDecision
- decisionId
- tenantId
- personId
- productKey
- decision
  - allow
  - deny
  - conditional
- reasonCode
  - tenant_inactive
  - entitlement_missing
  - entitlement_suspended
  - membership_missing
  - membership_suspended
  - account_disabled
  - grant_missing
  - grant_suspended
  - dependency_missing
  - allowed
- evaluatedAt
- entitlementSnapshot
- membershipSnapshot
- accountSnapshot
- grantSnapshot
```

## Tenant onboarding workflow

```text
1. Platform admin creates Tenant.
2. Tenant security policy is assigned.
3. ProductRegistry entries are available.
4. ProductEntitlements are granted.
5. Required dependencies are validated.
6. StaffArr/NexArr person/admin setup occurs.
7. Initial PlatformAccount/invitation is created.
8. Tenant becomes trial or active.
```

## Product entitlement workflow

```text
1. Admin selects tenant.
2. Admin selects product and tier.
3. NexArr validates product dependency rules.
4. NexArr creates ProductEntitlement.
5. NexArr grants or prepares product access.
6. Product receives entitlement changed event.
7. Product becomes available in launcher for allowed users.
```

## Product access workflow

```text
1. Person exists and has tenant membership.
2. Product entitlement exists for tenant.
3. Product access grant exists or is inherited.
4. Person launches product.
5. NexArr evaluates ProductAccessDecision.
6. If allowed, NexArr creates ProductLaunchSession.
7. If denied, NexArr shows reason and required action.
```

## Events

```text
nexarr.tenant.created
nexarr.tenant.updated
nexarr.tenant.status_changed
nexarr.tenant.suspended
nexarr.tenant.canceled
nexarr.tenant.archived

nexarr.membership.invited
nexarr.membership.activated
nexarr.membership.suspended
nexarr.membership.removed
nexarr.membership.expired

nexarr.product.registered
nexarr.product.updated
nexarr.product.status_changed

nexarr.entitlement.created
nexarr.entitlement.activated
nexarr.entitlement.suspended
nexarr.entitlement.expired
nexarr.entitlement.canceled
nexarr.entitlement.feature_flag_changed
nexarr.entitlement.usage_limit_changed

nexarr.product_access.granted
nexarr.product_access.suspended
nexarr.product_access.revoked
nexarr.product_access.expired
nexarr.product_access.decision_evaluated
```
