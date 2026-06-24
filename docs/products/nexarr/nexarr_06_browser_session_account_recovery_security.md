# NexArr — Browser Session, Account Recovery, and Security Hardening

## Audit mandate

Move refresh credentials out of JavaScript-readable storage, deliver enforceable SPA CSP, make refresh rotation atomic, protect MFA secrets at rest, and make password-reset delivery truthful.

## Browser session

Prefer a same-origin secure `HttpOnly` cookie/BFF model with CSRF protection and strict origin policy. Session and tenant changes clear client caches. Credentials never appear in URLs, logs, telemetry, or query keys.

## Refresh rotation

Rotation is one atomic transaction: validate current token, mark consumed, create child, and detect replay. Concurrent reuse revokes the family/session and emits a security event.

## MFA

Shared secrets are encrypted/protected at rest with key rotation and access audit. Recovery codes are hashed, single-use, and regenerated explicitly.

## Account recovery

Reset/invitation workflows record provider acceptance, delivery state, expiry, single use, and audit. The UI says “sent” only after configured delivery acceptance; otherwise it gives an honest alternate/admin path.

## Proxy and headers

Trust forwarded headers only from configured proxies. Define HTTPS/HSTS, canonical host, CSP, framing, referrer, content-type, and permissions policies for SPA and API responses.
