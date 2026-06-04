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
