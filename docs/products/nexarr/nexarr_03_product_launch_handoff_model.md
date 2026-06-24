# NexArr — Product Launch and Handoff Model

## Product launcher

The launcher is the central entry point to all active ordinary suite products.

```text
ProductLauncher
- tenantId
- userId
- personId nullable
- items
- defaultProductKey
- lastLaunchedProductKey
- generatedAt
```

## Launcher item

```text
ProductLauncherItem
- productKey
- displayName
- description
- iconKey
- launchUrl
- audience: tenant_member | platform_admin
- operationalStatus: available | degraded | maintenance | temporarily_unavailable
- statusMessage nullable
- notificationCountSnapshot nullable
- lastOpenedAt nullable
```

The launcher does not contain tenant/user grant or missing-license states. Compliance Core studio is omitted unless the session has validated platform-admin status.

## Product launch session

```text
ProductLaunchSession
- launchSessionId
- tenantId
- userId
- personId nullable
- productKey
- sessionId
- requestedAt
- issuedAt
- expiresAt
- returnUrl
- sourceProductKey
- destinationProductKey
- platformAdmin
- client/device context
- correlationId
- status: requested | issued | redeemed | expired | revoked | denied
- denialReason nullable
```

## Handoff token

The token is short-lived, one-time, audience-bound, and contains context—not domain permission.

```text
HandoffClaims
- issuer
- audienceProductKey
- tenantId
- userId
- personId nullable
- sessionId
- launchSessionId
- platformAdmin
- issuedAt / expiresAt
- nonce / tokenId
- returnUrl
- sourceProductKey
- correlationId
```

Do not embed product permissions or long-lived credentials. The destination resolves current permission context through its own trusted contracts.

## Launch workflow

1. User selects any ordinary product from the switcher.
2. NexArr validates account, active tenant membership, session/risk state, destination registry status, and return URL.
3. For Compliance Core studio only, NexArr validates platform-admin status.
4. NexArr creates a launch session and one-time handoff token.
5. Destination redeems the token server-side and establishes local session context.
6. Destination evaluates product permissions and record scope for each action.
7. Destination shows a permission-limited landing state when the user has little/no local authority; it does not claim the product is unavailable to the tenant.

## Launch denial

Valid denial reasons are tenant/account/membership/security/destination/platform-admin restrictions. “Missing product grant” and “product access grant missing” are invalid reasons.

## Product switching

Switching products preserves tenant/session context, creates a new destination-bound handoff, and clears destination-sensitive caches as needed. It does not require re-granting product access.

## Field Companion

Field Companion receives task-source context based on actual assignments/permissions. All source products remain launcher/runtime available; empty task lists are not interpreted as missing product availability.

## Events

- `nexarr.product_launch.requested`
- `nexarr.product_launch.issued`
- `nexarr.product_launch.redeemed`
- `nexarr.product_launch.denied`
- `nexarr.product_launch.expired`
- `nexarr.product_launch.revoked`
- `nexarr.product_switch.completed`
- `nexarr.compliancecore_studio_access.denied`
