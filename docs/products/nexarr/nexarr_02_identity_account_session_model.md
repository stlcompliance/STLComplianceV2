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
