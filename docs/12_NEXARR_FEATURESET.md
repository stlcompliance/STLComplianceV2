# NexArr Feature Set

## Product Definition

NexArr is the platform control plane for identity, tenant, entitlement, licensing, service clients, service tokens, and suite launch.

## Owns

- platform login
- auth credentials
- session-renewal tokens
- tenants
- product catalog
- entitlements
- licensing/subscriptions
- service clients
- service tokens
- platform admin
- launch authority
- platform audit

## Does Not Own

- people operational truth
- training evidence
- maintenance records
- routing records
- procurement records
- rule packs

## Core Features

- auth/login/logout/session renewal
- tenant management
- product catalog
- entitlement grants/revokes
- service client registration
- service token issuance/validation
- product launch context
- platform admin dashboard
- launch diagnostics
- audit search/export
- health/readiness
- hybrid data-plane metadata

## Required API Surfaces

- `/api/auth/login`
- `/api/auth/renew`
- `/api/auth/logout`
- `/api/me`
- `/api/me/tenants`
- `/api/me/entitlements`
- `/api/me/navigation`
- `/api/tenants`
- `/api/products`
- `/api/entitlements`
- `/api/service-tokens`
- `/api/platform-admin/*`
- `/health`

## Completion Definition

A non-technical admin can onboard a tenant, entitle products, govern service access, diagnose launch failures, and give users a simple product launcher.
