# NexArr Granular End-Goal Markdown Package

This package defines NexArr at the domain-object level.

## Files

- `nexarr_00_scope_and_boundaries.md`
- `nexarr_01_tenant_entitlement_model.md`
- `nexarr_02_identity_account_session_model.md`
- `nexarr_03_product_launch_handoff_model.md`
- `nexarr_04_service_client_token_security_model.md`
- `nexarr_05_workflows_status_events_apis.md`
- `nexarr_all_in_one_granular_featureset.md`

## Purpose

NexArr is the STL Compliance / ARR platform control plane.

It owns:

- Tenants
- Tenant membership validation
- Platform login
- Platform account security
- Login-capability linkage to person identity
- Product entitlement
- Product access grants
- Product launch and handoff
- Service clients
- Service tokens
- Platform security policy
- Platform audit

NexArr does not own product-domain operations, product-local authorization decisions after launch, maintenance work, inventory, routes, training completion, documents, quality holds, customer orders, or accounting execution.
