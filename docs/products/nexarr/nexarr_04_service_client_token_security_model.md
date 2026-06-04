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
