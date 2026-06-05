# NexArr — Scope, Ownership, and Boundaries

## Product purpose

NexArr is the platform control plane and secure front door for the STL Compliance / ARR suite. It owns tenant identity, login, platform account security, product entitlement, product launch, handoff, service-to-service trust, and platform audit.

NexArr answers:

- Does this tenant exist?
- Is this tenant active?
- Is this person allowed to log in?
- Is this person a member of this tenant?
- Is this product entitled for this tenant?
- Is this person allowed to launch this product?
- Can this service client call another product?
- What scopes does this service token carry?
- Was platform access granted, denied, expired, revoked, or suspicious?

NexArr does not answer domain questions such as:

- Can this technician close this specific work order?
- Can this driver start this trip?
- Can this warehouse user issue this part?
- Is this person qualified to operate this forklift?
- Can this inventory move while quality-held?
- Is this asset ready for use?

Those questions belong to StaffArr, TrainArr, MaintainArr, LoadArr, RoutArr, AssurArr, and other product domains.

## NexArr owns

```text
- Tenant
- Tenant status
- Tenant membership validation
- Platform account
- Login capability
- Password/security account fields
- MFA state
- Session state
- Product entitlement
- Product access grant
- Product launch session
- Product handoff token
- Product registry
- Product dependency rules
- Service client registry
- Service token issuance
- Service token scopes
- Platform security policy
- Platform audit events
- Platform admin surface
```

## NexArr does not own

```text
- StaffArr person profile details beyond login/account linkage snapshots
- Product-local permissions after launch
- StaffArr org structure
- StaffArr internal locations
- TrainArr training/certification truth
- Compliance Core rulepacks
- MaintainArr assets/work orders
- LoadArr inventory/stock ledger
- SupplyArr suppliers/procurement
- RoutArr routes/trips
- CustomArr customers
- OrdArr orders
- RecordArr documents/files
- AssurArr quality holds/CAPA
- ReportArr analytics read models
- Field Companion task execution truth
- Financial accounting execution
```

## External product dependencies

```text
StaffArr
- Person profile
- Person status
- Person org assignment
- Permission assignments
- Person readiness snapshot
- Internal locations

TrainArr
- Qualification/certification status
- Training completion status

Compliance Core
- Platform compliance rule references where needed
- Security/evidence requirements if platform actions need compliance review

RecordArr
- Platform policy/legal/security documents if stored as controlled records
- Audit package exports if needed

ReportArr
- Platform/admin analytics and cross-product reporting

All products
- Product registry
- Entitlement check
- Handoff token redemption
- Service token validation
- Product access launch context
```

## Core source-of-truth rules

```text
1. NexArr is the only acceptable login/authentication gate.
2. NexArr owns product entitlement.
3. NexArr owns product launch/handoff.
4. NexArr owns service clients and service tokens.
5. StaffArr owns the person profile and product-neutral permissions.
6. NexArr may reference personId but must not become the HR/person profile system.
7. Products own product-domain authorization after NexArr launch.
8. Products must not implement their own platform login.
9. Products must validate handoff/service token context.
10. Products must enforce tenant isolation after handoff.
11. NexArr must not bypass product-domain rules.
12. Platform admin belongs in NexArr.
```

## Standard NexArr object envelope

```text
NexArrObject
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
- sourceIp
- userAgent
- correlationId
- auditTrail
- eventLog
```

## NexArr object prefixes

```text
TEN    Tenant
MEM    Tenant membership
ACC    Platform account
SESS   Platform session
MFA    MFA method/challenge
ENT    Product entitlement
PAG    Product access grant
PROD   Product registry entry
LAUN   Product launch session
HAND   Handoff token
SVC    Service client
TOK    Service token
SCOPE  Service/product scope
POL    Security policy
AUD    Platform audit entry
INV    Invitation
```

## Platform identity rule

```text
Person identity
- personId is the platform human identifier used across products.
- StaffArr owns the human profile and people/org/person history.
- NexArr owns whether that person can log in.
- NexArr owns platform credentials/security.
- Products reference personId, not product-local user IDs as human truth.
```

## Login capability rule

```text
A person can log in only when:
- personId exists
- tenant membership is valid
- PlatformAccount exists
- hasUserAccount is true
- canLogin is true
- account status allows login
- credential/SSO/MFA requirements are satisfied
- tenant/product entitlement rules allow entry to the target product
```


---


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
  - FieldCompanion
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


---


# NexArr — Identity, Account, Session, and MFA Model

## Platform account

A PlatformAccount is the login/security record for a person within a tenant context. It should link to `personId`; it should not replace StaffArr’s person profile.

```text
PlatformAccount
- platformAccountId
- tenantId
- personId
- email
- username
- status
  - invited
  - active
  - locked
  - disabled
  - password_reset_required
  - archived
- hasUserAccount
- canLogin
- authenticationType
  - password
  - sso
  - password_and_sso
  - service_only
- passwordHash
- passwordUpdatedAt
- passwordResetRequired
- passwordResetRequestedAt
- passwordResetTokenHash
- passwordResetTokenExpiresAt
- mfaRequired
- mfaEnabled
- mfaMethodRefs
- failedLoginCount
- lastFailedLoginAt
- lastLoginAt
- lockedAt
- lockedBy
  - system
  - admin
- lockReason
- disabledAt
- disabledByPersonId
- disableReason
- termsAcceptedAt
- privacyAcceptedAt
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Account status definitions

```text
invited
- Account invitation exists but user has not activated.

active
- Account can authenticate if security requirements are met.

locked
- Login is blocked by security lockout.

disabled
- Login is administratively disabled.

password_reset_required
- User must reset password before normal access.

archived
- Account retained for history only.
```

## Login identifier

```text
LoginIdentifier
- loginIdentifierId
- platformAccountId
- identifierType
  - email
  - username
  - phone
  - external_sso_subject
- identifierValue
- verified
- primary
- status
  - active
  - inactive
  - revoked
```

## Invitation

```text
Invitation
- invitationId
- tenantId
- personId
- platformAccountId
- email
- invitationType
  - new_account
  - tenant_membership
  - product_access
  - admin_invite
- status
  - created
  - sent
  - accepted
  - expired
  - revoked
- invitedByPersonId
- invitedAt
- sentAt
- acceptedAt
- expiresAt
- revokedAt
- revokeReason
- productAccessGrantRefs
```

## MFA method

```text
MfaMethod
- mfaMethodId
- platformAccountId
- methodType
  - authenticator_app
  - sms
  - email
  - security_key
  - backup_code
- status
  - pending
  - active
  - disabled
  - revoked
- displayName
- secretHashOrPublicKeyRef
- createdAt
- verifiedAt
- lastUsedAt
- revokedAt
```

## MFA challenge

```text
MfaChallenge
- mfaChallengeId
- platformAccountId
- methodType
- status
  - created
  - verified
  - failed
  - expired
  - canceled
- createdAt
- expiresAt
- verifiedAt
- failedAt
- failureReason
- sourceIp
- userAgent
```

## Platform session

```text
PlatformSession
- platformSessionId
- tenantId
- personId
- platformAccountId
- status
  - active
  - expired
  - revoked
  - replaced
- sessionType
  - web
  - mobile
  - api
  - handoff
- createdAt
- expiresAt
- lastSeenAt
- revokedAt
- revokedByPersonId
- revokeReason
- sourceIp
- userAgent
- deviceFingerprint
- mfaSatisfied
- currentProductKey
- refreshTokenRef
```

## Refresh token

```text
RefreshToken
- refreshTokenId
- platformSessionId
- tokenHash
- status
  - active
  - rotated
  - revoked
  - expired
- issuedAt
- expiresAt
- rotatedAt
- revokedAt
- revokeReason
```

## Login attempt

```text
LoginAttempt
- loginAttemptId
- tenantId
- identifier
- platformAccountId
- result
  - success
  - failed
  - mfa_required
  - mfa_failed
  - locked
  - disabled
  - tenant_inactive
  - unknown_account
- reasonCode
- occurredAt
- sourceIp
- userAgent
- geoSnapshot
- deviceFingerprint
- riskScore
```

## Account lockout policy

```text
AccountLockoutPolicy
- policyId
- tenantId
- maxFailedAttempts
- lockoutDurationMinutes
- resetWindowMinutes
- notifyUser
- notifyAdmins
- status
```

## Password policy

```text
PasswordPolicy
- policyId
- tenantId
- minimumLength
- requireUppercase
- requireLowercase
- requireNumber
- requireSymbol
- preventReuseCount
- expirationDays
- breachedPasswordCheckEnabled
- status
```

## SSO provider

```text
SsoProvider
- ssoProviderId
- tenantId
- providerType
  - oidc
  - saml
  - google
  - microsoft
  - custom
- displayName
- status
  - draft
  - active
  - disabled
- issuer
- clientId
- metadataUrl
- certificateRefs
- allowedDomains
- autoProvisionEnabled
- defaultMembershipType
- defaultProductAccessRules
- createdAt
- updatedAt
```

## Authentication workflow

```text
1. User submits login identifier.
2. NexArr resolves PlatformAccount.
3. NexArr validates tenant membership.
4. NexArr checks account status.
5. NexArr validates password or SSO assertion.
6. NexArr applies lockout/risk policy.
7. NexArr prompts MFA if required.
8. NexArr creates PlatformSession.
9. NexArr records LoginAttempt and audit entry.
10. User sees product launcher.
```

## Account invitation workflow

```text
1. Admin creates or selects person.
2. Admin decides canLogin/hasUserAccount.
3. NexArr creates PlatformAccount if needed.
4. NexArr creates Invitation.
5. User accepts invitation.
6. User sets credentials or completes SSO.
7. MFA/terms/privacy are completed if required.
8. Account becomes active.
```

## Password reset workflow

```text
1. User requests password reset.
2. NexArr creates password reset token.
3. User verifies token.
4. User sets new password.
5. NexArr rotates/revokes relevant sessions if policy requires.
6. Login resumes.
```

## Session revocation workflow

```text
1. User/admin/security policy revokes session.
2. PlatformSession becomes revoked.
3. RefreshToken becomes revoked.
4. Product handoffs using that session fail.
5. Audit entry is created.
```

## Events

```text
nexarr.account.created
nexarr.account.invited
nexarr.account.activated
nexarr.account.updated
nexarr.account.locked
nexarr.account.unlocked
nexarr.account.disabled
nexarr.account.password_reset_requested
nexarr.account.password_changed
nexarr.account.mfa_enabled
nexarr.account.mfa_disabled

nexarr.login.succeeded
nexarr.login.failed
nexarr.login.mfa_required
nexarr.login.mfa_failed
nexarr.login.lockout_triggered

nexarr.session.created
nexarr.session.refreshed
nexarr.session.revoked
nexarr.session.expired

nexarr.invitation.created
nexarr.invitation.sent
nexarr.invitation.accepted
nexarr.invitation.expired
nexarr.invitation.revoked

nexarr.sso_provider.created
nexarr.sso_provider.activated
nexarr.sso_provider.disabled
```


---


# NexArr — Product Launch and Handoff Model

## Product launcher

The product launcher is the user’s central entry point into entitled products.

```text
ProductLauncher
- launcherId
- tenantId
- personId
- availableProductRefs
- deniedProductRefs
- defaultProductKey
- lastLaunchedProductKey
- generatedAt
```

## Product launcher item

```text
ProductLauncherItem
- productKey
- displayName
- description
- iconKey
- launchUrl
- status
  - available
  - denied
  - suspended
  - missing_entitlement
  - dependency_missing
  - account_blocked
- denialReason
- featureFlags
- notificationCountSnapshot
- lastOpenedAt
```

## Product launch session

A ProductLaunchSession is created when a user attempts to enter a product.

```text
ProductLaunchSession
- launchSessionId
- tenantId
- personId
- platformAccountId
- productKey
- requestedPath
- returnUrl
- deepLinkPath
- tenantHint
- launchContext
- status
  - created
  - redeemed
  - expired
  - rejected
  - canceled
- accessDecisionRef
- handoffTokenRef
- createdAt
- expiresAt
- redeemedAt
- rejectedAt
- rejectionReason
- sourceIp
- userAgent
- correlationId
```

## Handoff token

A HandoffToken is a short-lived signed token used to transfer platform-authenticated context to a product.

```text
HandoffToken
- handoffTokenId
- launchSessionId
- tenantId
- personId
- productKey
- tokenHash
- status
  - active
  - redeemed
  - expired
  - revoked
  - rejected
- issuedAt
- expiresAt
- redeemedAt
- redeemedByProduct
- audience
- scopes
- claimsSnapshot
- sourceIp
- userAgent
```

## Handoff claims

```text
HandoffClaims
- tenantId
- personId
- platformAccountId
- productKey
- launchSessionId
- issuedAt
- expiresAt
- nonce
- audience
- returnUrl
- deepLinkPath
- entitlementSnapshot
- productAccessGrantSnapshot
- staffarrPermissionHint
- correlationId
```

## Handoff redemption

```text
HandoffRedemption
- redemptionId
- handoffTokenId
- productKey
- tenantId
- personId
- status
  - accepted
  - rejected
  - expired
  - duplicate
  - invalid_signature
  - wrong_audience
- redeemedAt
- productSessionRef
- rejectionReason
- sourceIp
- userAgent
```

## Return URL policy

```text
ReturnUrlPolicy
- policyId
- tenantId
- productKey
- allowedReturnUrlPatterns
- allowedDeepLinkPatterns
- defaultReturnUrl
- status
```

## Product session reference

NexArr does not own the product’s internal session, but it may store a reference for audit/visibility.

```text
ProductSessionRef
- productSessionRefId
- tenantId
- personId
- productKey
- productSessionIdSnapshot
- launchSessionId
- statusSnapshot
- startedAt
- endedAt
- lastSeenAt
```

## Product notification summary

```text
ProductNotificationSummary
- tenantId
- personId
- productKey
- notificationCount
- urgentCount
- blockedCount
- lastUpdatedAt
```

## Launch workflow

```text
1. User logs in.
2. NexArr builds ProductLauncher.
3. User selects product.
4. NexArr validates tenant status.
5. NexArr validates product entitlement.
6. NexArr validates product dependency rules.
7. NexArr validates product access grant.
8. NexArr creates ProductLaunchSession.
9. NexArr creates HandoffToken.
10. Browser/app redirects to product launch URL.
11. Product redeems handoff token with NexArr.
12. Product creates local product session.
13. Product loads StaffArr permissions/readiness context.
14. Product opens deep link or default workspace.
```

## Launch denial workflow

```text
1. User selects product.
2. NexArr evaluates ProductAccessDecision.
3. Decision is deny or conditional.
4. NexArr shows product card with denial reason.
5. User may request access if enabled.
6. Admin can grant entitlement/access if appropriate.
```

## Handoff redemption workflow

```text
1. Product receives handoff token.
2. Product calls NexArr redemption endpoint.
3. NexArr validates signature/token hash/status/expiry/audience.
4. NexArr marks token redeemed.
5. NexArr returns claims/context to product.
6. Product validates tenant/product context.
7. Product creates local session.
8. Product enforces product-local authorization.
```

## Product switcher workflow

```text
1. User is inside product.
2. User opens suite/product switcher.
3. Product requests available launcher context from NexArr or cached context.
4. User selects another product.
5. NexArr creates new ProductLaunchSession.
6. User is handed to target product.
```

## Field Companion launch workflow

```text
1. User opens Field Companion.
2. NexArr validates mobile session/login.
3. NexArr returns entitled product surfaces.
4. Field Companion shows available source-product actions.
5. Field Companion still calls source product APIs for task execution.
```

## Events

```text
nexarr.launcher.generated
nexarr.product_launch.requested
nexarr.product_launch.created
nexarr.product_launch.denied
nexarr.product_launch.redeemed
nexarr.product_launch.expired
nexarr.product_launch.rejected

nexarr.handoff_token.issued
nexarr.handoff_token.redeemed
nexarr.handoff_token.expired
nexarr.handoff_token.revoked
nexarr.handoff_redemption.accepted
nexarr.handoff_redemption.rejected

nexarr.product_session.started
nexarr.product_session.ended
nexarr.product_switch.requested
```


---


# NexArr — Service Client, Token, Scope, and Security Model

## Service client

A ServiceClient represents a product/service identity used for service-to-service calls.

```text
ServiceClient
- serviceClientId
- clientKey
- displayName
- description
- owningProduct
- status
  - draft
  - active
  - disabled
  - revoked
  - archived
- clientType
  - product_api
  - background_worker
  - integration
  - reporting
  - automation
  - internal_tool
- allowedScopes
- allowedTenantIds
- allowedAudiences
- tokenLifetimeSeconds
- tokenRotationPolicyRef
- secretHashRefs
- certificateRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- lastUsedAt
- revokedAt
- revokedByPersonId
- revokeReason
```

## Service client secret

```text
ServiceClientSecret
- secretId
- serviceClientId
- secretHash
- status
  - active
  - rotated
  - revoked
  - expired
- createdAt
- expiresAt
- rotatedAt
- revokedAt
- lastUsedAt
```

## Service scope

A ServiceScope is a machine-usable permission for product-to-product integration.

```text
ServiceScope
- scopeId
- scopeKey
- displayName
- description
- productKey
- category
  - read
  - write
  - event
  - admin
  - integration
- riskLevel
  - low
  - moderate
  - high
  - critical
- status
  - active
  - deprecated
  - retired
```

## Service token

```text
ServiceToken
- serviceTokenId
- serviceClientId
- tenantId
- tokenHash
- status
  - active
  - rotated
  - revoked
  - expired
- scopes
- audience
- issuedAt
- expiresAt
- revokedAt
- revokedByPersonId
- revokeReason
- lastUsedAt
- sourceIp
- correlationId
```

## Service token introspection

```text
ServiceTokenIntrospection
- introspectionId
- serviceTokenId
- serviceClientId
- tenantId
- audience
- active
- scopes
- status
- checkedAt
- checkedByProduct
- result
  - valid
  - invalid
  - expired
  - revoked
  - wrong_audience
  - insufficient_scope
```

## Service call audit

```text
ServiceCallAudit
- serviceCallAuditId
- tenantId
- sourceServiceClientId
- sourceProduct
- targetProduct
- targetEndpoint
- method
- scopesUsed
- result
  - allowed
  - denied
  - failed
- statusCode
- occurredAt
- correlationId
- sourceIp
- reasonCode
```

## Platform security policy

```text
PlatformSecurityPolicy
- securityPolicyId
- tenantId
- policyName
- status
  - active
  - inactive
  - archived
- passwordPolicyRef
- mfaPolicyRef
- sessionPolicyRef
- lockoutPolicyRef
- serviceTokenPolicyRef
- allowedDomainRules
- ipAllowlistRules
- deviceTrustPolicy
- auditRetentionPolicy
- createdAt
- updatedAt
```

## MFA policy

```text
MfaPolicy
- mfaPolicyId
- tenantId
- requiredForAllUsers
- requiredForAdmins
- requiredForHighRiskActions
- allowedMethods
- rememberDeviceDays
- backupCodesAllowed
- status
```

## Session policy

```text
SessionPolicy
- sessionPolicyId
- tenantId
- webSessionLifetimeMinutes
- mobileSessionLifetimeMinutes
- idleTimeoutMinutes
- refreshTokenLifetimeDays
- revokeOnPasswordChange
- singleSessionOnly
- status
```

## Service token policy

```text
ServiceTokenPolicy
- serviceTokenPolicyId
- tenantId
- defaultLifetimeSeconds
- maxLifetimeSeconds
- rotationRequired
- rotationIntervalDays
- allowLongLivedTokens
- allowedClientTypes
- status
```

## IP allowlist rule

```text
IpAllowlistRule
- ipAllowlistRuleId
- tenantId
- ruleName
- cidr
- appliesTo
  - admin
  - all_users
  - service_clients
  - product_launch
- status
```

## Audit entry

```text
PlatformAuditEntry
- auditEntryId
- tenantId
- actorPersonId
- actorServiceClientId
- action
- objectType
- objectId
- result
  - success
  - denied
  - failed
- reasonCode
- beforeSnapshot
- afterSnapshot
- occurredAt
- sourceIp
- userAgent
- correlationId
```

## Suspicious activity signal

```text
SuspiciousActivitySignal
- signalId
- tenantId
- personId
- platformAccountId
- serviceClientId
- signalType
  - failed_login_spike
  - impossible_travel
  - unusual_ip
  - unusual_device
  - token_reuse
  - revoked_token_used
  - excessive_launch_denials
  - suspicious_service_call
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - investigating
  - resolved
  - dismissed
- detectedAt
- resolvedAt
- resolvedByPersonId
- evidence
```

## Service token issuance workflow

```text
1. Source product authenticates as ServiceClient.
2. NexArr validates client status and secret/cert.
3. Source product requests tenant/audience/scopes.
4. NexArr validates allowed scopes and tenant access.
5. NexArr issues ServiceToken.
6. Source product calls target product.
7. Target product introspects or validates token.
8. Target product enforces scope and product-local rules.
9. Service call audit is recorded.
```

## Scope validation workflow

```text
1. Target product receives service call.
2. Target product checks token signature or introspects token.
3. Target product validates audience.
4. Target product validates tenant.
5. Target product validates required scope.
6. Target product performs action or rejects.
```

## Token rotation workflow

```text
1. Rotation policy reaches threshold.
2. NexArr flags client secret/token for rotation.
3. Admin or automation creates new secret.
4. Product updates configuration.
5. Old secret/token is revoked after overlap period.
6. Audit entry is recorded.
```

## Suspicious activity workflow

```text
1. NexArr detects suspicious signal.
2. Signal is scored.
3. Account/session/token may be locked/revoked depending policy.
4. Admin/security notification is created.
5. Admin reviews and resolves/dismisses.
6. Audit trail is retained.
```

## Events

```text
nexarr.service_client.created
nexarr.service_client.activated
nexarr.service_client.disabled
nexarr.service_client.revoked

nexarr.service_scope.created
nexarr.service_scope.updated
nexarr.service_token.issued
nexarr.service_token.introspected
nexarr.service_token.revoked
nexarr.service_token.expired

nexarr.security_policy.created
nexarr.security_policy.updated
nexarr.ip_allowlist.updated

nexarr.audit.entry_created
nexarr.suspicious_activity.detected
nexarr.suspicious_activity.resolved
```


---


# NexArr — Workflows, Status Logic, Events, and APIs

## Major workflow: tenant onboarding

```text
1. Platform admin creates Tenant.
2. Tenant is assigned status trial or active.
3. Security policy is selected or defaulted.
4. Product entitlements are created.
5. Product dependencies are validated.
6. Initial person/admin is linked.
7. PlatformAccount is created.
8. Invitation is sent.
9. Admin accepts invitation and logs in.
10. NexArr displays product launcher.
```

## Major workflow: login

```text
1. User enters identifier.
2. NexArr resolves PlatformAccount.
3. NexArr validates tenant membership.
4. NexArr validates account status.
5. NexArr validates credentials or SSO.
6. NexArr applies lockout/risk policy.
7. NexArr requires MFA if policy says so.
8. NexArr creates PlatformSession.
9. NexArr records audit and login attempt.
10. User enters launcher.
```

## Major workflow: product launch

```text
1. User selects product from launcher.
2. NexArr checks tenant status.
3. NexArr checks ProductEntitlement.
4. NexArr checks ProductDependencyRule.
5. NexArr checks ProductAccessGrant.
6. NexArr creates ProductAccessDecision.
7. If allowed, NexArr creates ProductLaunchSession.
8. NexArr creates HandoffToken.
9. User is redirected to product.
10. Product redeems token.
11. Product creates local session.
12. Product loads StaffArr/product-local permissions.
```

## Major workflow: product access denied

```text
1. User selects product.
2. NexArr denies launch.
3. Denial reason is shown.
4. Optional request-access workflow starts.
5. Admin grants entitlement/access if appropriate.
6. Product launcher updates.
```

## Major workflow: service-to-service call

```text
1. Product authenticates as ServiceClient.
2. Product requests ServiceToken.
3. NexArr validates client, tenant, scopes, and audience.
4. NexArr issues token.
5. Source product calls target product.
6. Target product validates token.
7. Target product validates scope.
8. Target product performs action or rejects.
9. Service audit is retained.
```

## Major workflow: account disable/termination sync

```text
1. StaffArr terminates or suspends person.
2. StaffArr publishes person status event.
3. NexArr disables or suspends PlatformAccount according to policy.
4. NexArr revokes active sessions if required.
5. Product access grants may be revoked/suspended.
6. Products stop accepting new product sessions for that person.
```

## Major workflow: entitlement suspension

```text
1. Platform admin or billing/integration suspends ProductEntitlement.
2. NexArr marks entitlement suspended.
3. Product launch is denied for that product.
4. Product receives entitlement suspended event.
5. Existing product sessions may be allowed to expire or revoked depending policy.
```

## NexArr emitted events

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

nexarr.account.created
nexarr.account.invited
nexarr.account.activated
nexarr.account.updated
nexarr.account.locked
nexarr.account.unlocked
nexarr.account.disabled
nexarr.account.password_reset_requested
nexarr.account.password_changed
nexarr.account.mfa_enabled
nexarr.account.mfa_disabled

nexarr.login.succeeded
nexarr.login.failed
nexarr.login.mfa_required
nexarr.login.mfa_failed
nexarr.login.lockout_triggered

nexarr.session.created
nexarr.session.refreshed
nexarr.session.revoked
nexarr.session.expired

nexarr.product.registered
nexarr.entitlement.created
nexarr.entitlement.activated
nexarr.entitlement.suspended
nexarr.entitlement.expired
nexarr.entitlement.canceled

nexarr.product_access.granted
nexarr.product_access.suspended
nexarr.product_access.revoked
nexarr.product_access.decision_evaluated

nexarr.product_launch.created
nexarr.product_launch.denied
nexarr.product_launch.redeemed
nexarr.handoff_token.issued
nexarr.handoff_token.redeemed
nexarr.handoff_token.expired
nexarr.handoff_redemption.rejected

nexarr.service_client.created
nexarr.service_token.issued
nexarr.service_token.revoked
nexarr.service_token.expired

nexarr.security_policy.updated
nexarr.audit.entry_created
nexarr.suspicious_activity.detected
```

## Integration APIs NexArr should expose

```text
GET /api/v1/platform/me
GET /api/v1/platform/me/products
GET /api/v1/platform/me/sessions

GET /api/v1/platform/tenants
GET /api/v1/platform/tenants/{tenantId}
POST /api/v1/platform/tenants
PATCH /api/v1/platform/tenants/{tenantId}

GET /api/v1/platform/tenants/{tenantId}/memberships
POST /api/v1/platform/tenants/{tenantId}/memberships
PATCH /api/v1/platform/memberships/{membershipId}

GET /api/v1/platform/accounts/by-person/{personId}
POST /api/v1/platform/accounts
PATCH /api/v1/platform/accounts/{platformAccountId}
POST /api/v1/platform/accounts/{platformAccountId}/disable
POST /api/v1/platform/accounts/{platformAccountId}/unlock
POST /api/v1/platform/accounts/{platformAccountId}/password-reset

POST /api/v1/platform/login
POST /api/v1/platform/logout
POST /api/v1/platform/sessions/refresh
POST /api/v1/platform/sessions/{sessionId}/revoke

GET /api/v1/platform/products
GET /api/v1/platform/products/{productKey}
POST /api/v1/platform/products

GET /api/v1/platform/tenants/{tenantId}/entitlements
GET /api/v1/platform/tenants/{tenantId}/entitlements/{productKey}
POST /api/v1/platform/tenants/{tenantId}/entitlements
PATCH /api/v1/platform/entitlements/{entitlementId}

GET /api/v1/platform/product-access
POST /api/v1/platform/product-access/grants
POST /api/v1/platform/product-access/decisions
POST /api/v1/platform/product-access/revoke

POST /api/v1/platform/launch
POST /api/v1/platform/handoff/redeem
POST /api/v1/platform/handoff/revoke

GET /api/v1/platform/service-clients
POST /api/v1/platform/service-clients
PATCH /api/v1/platform/service-clients/{serviceClientId}
POST /api/v1/platform/service-clients/{serviceClientId}/rotate-secret
POST /api/v1/platform/service-clients/{serviceClientId}/revoke

POST /api/v1/platform/service-tokens
POST /api/v1/platform/service-tokens/introspect
POST /api/v1/platform/service-tokens/{serviceTokenId}/revoke

GET /api/v1/platform/security-policy
PATCH /api/v1/platform/security-policy
GET /api/v1/platform/audit
```

## APIs NexArr should consume

```text
StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/summary
- GET /persons/{personId}/permissions
- POST /person-history-events

RecordArr
- POST /records
- GET /records/{recordId}
- POST /record-packages

ReportArr
- POST /events

Optional product health endpoints
- GET /health
- GET /version
- GET /status
```

## Permission examples

```text
nexarr.platform.read
nexarr.platform.admin

nexarr.tenants.read
nexarr.tenants.create
nexarr.tenants.update
nexarr.tenants.suspend
nexarr.tenants.cancel

nexarr.memberships.read
nexarr.memberships.invite
nexarr.memberships.suspend
nexarr.memberships.remove

nexarr.accounts.read
nexarr.accounts.create
nexarr.accounts.update
nexarr.accounts.disable
nexarr.accounts.unlock
nexarr.accounts.reset_password

nexarr.products.read
nexarr.products.manage

nexarr.entitlements.read
nexarr.entitlements.grant
nexarr.entitlements.update
nexarr.entitlements.suspend
nexarr.entitlements.cancel

nexarr.product_access.read
nexarr.product_access.grant
nexarr.product_access.revoke

nexarr.service_clients.read
nexarr.service_clients.manage
nexarr.service_tokens.issue
nexarr.service_tokens.revoke

nexarr.security_policy.read
nexarr.security_policy.manage
nexarr.audit.read
```

## Default role examples

```text
Platform Viewer
- Read tenants, products, entitlement summaries, and audit summaries.

Tenant Admin
- Manage own tenant membership and product access grants within allowed scope.

Platform Admin
- Manage tenants, product entitlements, product registry, platform access, and service clients.

Security Admin
- Manage security policies, sessions, MFA/account lockouts, and suspicious activity.

Service Integration Admin
- Manage service clients, scopes, token rotation, and integration access.

Support Admin
- View tenant/account/product access status for support without broad destructive rights.

Auditor
- Read platform audit, launch history, entitlement history, and access decisions.
```

## NexArr UI surfaces

```text
/app/nexarr
- dashboard
- product launcher
- tenants
- tenant detail
- memberships
- accounts
- product registry
- entitlements
- product access grants
- launch history
- service clients
- service tokens
- scopes
- security policies
- sessions
- login audit
- suspicious activity
- platform audit
- settings
```

## Tenant detail UI

```text
TenantDetailPage
- Tenant header
- Status
- Deployment mode
- Product entitlements
- Members
- Product access grants
- Security policy
- Service clients
- Usage/limits snapshot
- Audit history
```

## Account detail UI

```text
AccountDetailPage
- Account header
- Person reference
- Tenant memberships
- Login identifiers
- MFA methods
- Sessions
- Product access
- Login attempts
- Lock/disable controls
- Audit history
```

## Product entitlement UI

```text
ProductEntitlementPage
- Tenant
- Product
- Status
- Tier
- Feature flags
- Usage limits
- Dependencies
- Access grants
- Event history
```

## Service client UI

```text
ServiceClientPage
- Client identity
- Owning product
- Status
- Allowed scopes
- Allowed tenants
- Token policy
- Secrets/certificates
- Rotation status
- Last used
- Audit history
```
