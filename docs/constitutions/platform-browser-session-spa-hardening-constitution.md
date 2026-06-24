# STL Compliance Browser Session and SPA Hardening Constitution

## 1. Audit drivers

SEC-005, SEC-009 through SEC-011, SEC-013 through SEC-018 identified JavaScript-readable refresh credentials, missing SPA CSP, refresh rotation races, plaintext MFA secrets, misleading reset delivery, proxy ambiguity, capability-policy conflicts, and cache-key credential exposure.

## 2. Prime directive

A browser session must remain safe under ordinary SPA threats, explicit about proxy trust, and honest about recovery/delivery behavior.

## 3. Credential storage

Long-lived or refresh credentials may not be stored in `localStorage`, `sessionStorage`, URL parameters, query-cache keys, or JavaScript-readable persistent stores. Prefer same-origin secure `HttpOnly`, `Secure`, appropriately `SameSite` cookies through a backend-for-frontend/session design.

Short-lived access state kept in memory must be cleared on logout, tenant switch, session rotation, and account disable.

## 4. CSRF and origins

Cookie-authenticated mutations require CSRF protection and strict origin validation. CORS allowlists must be explicit; wildcard subdomain trust is prohibited unless each subdomain is equally trusted and controlled.

## 5. CSP and document headers

Every SPA HTML response receives an enforceable CSP header, including `frame-ancestors`, script/style nonce or hashes, object/base restrictions, and trusted connection targets. Meta-only framing controls are insufficient.

## 6. Token and MFA controls

- refresh rotation is atomic and reuse-detecting
- MFA shared secrets are encrypted/protected at rest
- reset/recovery tokens are single-use, short-lived, and auditable
- the UI may claim an email/message was sent only after a configured delivery provider accepts it
- session/device revocation propagates across products

## 7. Proxy and transport

Forwarded headers are accepted only from trusted proxies/networks. HTTPS enforcement, HSTS, canonical host, secure-cookie behavior, and client-IP attribution must be explicit for each deployment topology.

## 8. Browser capability policy

Permissions Policy must allow only the capabilities required by each product origin. Field Companion geolocation/camera workflows may not be globally disabled by the shared host. Capability denial must degrade clearly.

## 9. Client caches

Credentials may not be part of query keys, logs, telemetry attributes, analytics, error payloads, or URLs. Tenant/session changes must clear or partition caches to prevent cross-context data display.
