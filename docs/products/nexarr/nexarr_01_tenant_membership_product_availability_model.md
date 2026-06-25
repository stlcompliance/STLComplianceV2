# NexArr — Tenant Membership and Product Availability Model

## Tenant

A Tenant is the platform customer and isolation boundary.

```text
Tenant
- tenantId
- tenantNumber
- legalName
- displayName
- slug
- status: prospect | trial | active | suspended | canceled | archived
- tenantType: internal | customer | demo | sandbox | partner | test
- primaryContactPersonId
- ownerPersonId
- defaultTimezone
- defaultLocale
- dataRegion
- deploymentMode: hosted | hybrid | customer_hosted_data_plane
- securityPolicyRef
- billingAccountExternalRef
- supportTier
- createdAt / createdByPersonId
- updatedAt / updatedByPersonId
- suspension/cancellation/archive metadata
- auditTrail
```

Tenant status affects the tenant as a whole, not individual product availability. An active/trial tenant may launch every ordinary product; a suspended/canceled/archived tenant follows platform launch policy consistently across products.

## Tenant membership

```text
TenantMembership
- membershipId
- tenantId
- userId
- personId nullable until linked
- status: invited | active | suspended | removed | expired
- membershipType: employee | contractor | vendor | customer | auditor | admin
- invitedAt / invitedByPersonId
- joinedAt
- suspendedAt / suspendedByPersonId / reason
- removedAt / removedByPersonId / reason
- expiresAt
- sourceProduct / sourceObjectRef
- createdAt / updatedAt
```

Membership decides whether an account can act for a tenant. It does not decide which ordinary products appear.

## Account-to-person link

```text
AccountPersonLink
- linkId
- tenantId
- userId
- personId
- status: pending | active | disputed | superseded | removed
- source: staffarr_provisioned | nexarr_invitation | admin_link | migration
- verifiedAt / verifiedByPersonId
- createdAt / updatedAt
```

`userId` and `personId` remain distinct. Product audit context may include both.

## Product registry

The registry is a platform catalog, not a tenant access list.

```text
ProductRegistryEntry
- productRegistryEntryId
- productKey
- displayName
- description
- category
- audience: tenant_member | platform_admin | public
- status: active | maintenance | suspended | retired
- launchUrl
- apiBaseUrl
- healthCheckUrl
- iconKey
- canonicalOrder
- requiredOperationalDependencies
- optionalOperationalDependencies
- createdAt / updatedAt
```

Canonical tenant-member products include NexArr/suite account surfaces, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, LoadArr, AssurArr, RecordArr, ReportArr, OrdArr, CustomArr, LedgArr, and Field Companion as applicable to the current form factor.

Compliance Core studio uses `audience = platform_admin`. Its runtime services are not a launcher restriction.

The STL Compliance public site uses `audience = public` and is not a tenant product-switcher item.

## Product operational state

```text
ProductOperationalState
- productKey
- status: available | degraded | maintenance | temporarily_unavailable
- message
- startedAt
- expectedResolutionAt nullable
- incidentRef nullable
- updatedAt
```

Operational state explains deployment health. It never means a tenant is unlicensed or a user is blocked by product availability.

## Launcher decision

```text
ProductLaunchDecision
- tenantId
- userId
- productKey
- decision: allowed | tenant_inactive | membership_inactive | destination_inactive | platform_admin_required | security_blocked
- reasonCode
- message
- productOperationalState
- generatedAt
- correlationId
```

For an active ordinary product, an active tenant member is allowed to launch. The destination product still evaluates action permissions.

## Product dependency behavior

Dependencies affect feature readiness or degraded state, not launcher membership. If an owner product is down, the destination opens and explains which workflow is degraded unless safe operation requires a full outage.

## Tenant onboarding

1. Create tenant and security policy.
2. Create/invite initial membership and link person when available.
3. Configure tenant-wide and product-owned settings.
4. Display all ordinary products in the launcher.
5. Establish StaffArr roles/permissions and product setup checklists.
6. Invoke Compliance Core onboarding questionnaires through ordinary product/onboarding surfaces without granting studio access.

## Events

- `nexarr.tenant.created`
- `nexarr.tenant.activated`
- `nexarr.tenant.suspended`
- `nexarr.membership.invited`
- `nexarr.membership.activated`
- `nexarr.membership.suspended`
- `nexarr.membership.removed`
- `nexarr.account_person_link.verified`
- `nexarr.product_registry.updated`
- `nexarr.product_operational_state.changed`
- `nexarr.platform_admin.granted`
- `nexarr.platform_admin.revoked`

There are no product-membership and operational-state events.
