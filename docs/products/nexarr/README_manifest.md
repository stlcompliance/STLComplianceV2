# NexArr Granular End-Goal Markdown Package

This package defines NexArr as the platform identity, tenant-membership, session, launch-context, service-trust, and platform-administration control plane.

## Files

- `nexarr_00_scope_and_boundaries.md`
- `nexarr_01_tenant_membership_product_availability_model.md`
- `nexarr_02_identity_account_session_model.md`
- `nexarr_03_product_launch_handoff_model.md`
- `nexarr_04_service_client_token_security_model.md`
- `nexarr_05_workflows_status_events_apis.md`
- `nexarr_06_browser_session_account_recovery_security.md`

## Purpose

NexArr owns:

- tenant identity and status
- tenant membership
- platform accounts, login, MFA, sessions, and recovery
- static product registry and launch destinations
- launch/handoff context
- platform-admin status and platform administration
- service clients, service tokens, and scopes
- platform access and security audit

Product availability is nonvariable. Every active tenant member can launch every ordinary STL Compliance product. There is no tenant product grant or per-user product launch grant. Compliance Core’s administrative studio is the only product UI reserved for platform administrators; Compliance Core runtime services remain available to all tenants and users through authorized product workflows.

NexArr does not own product-domain permissions, StaffArr person truth, or domain records.
