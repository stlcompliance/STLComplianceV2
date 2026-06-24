# Constitution Compliance Checklist

Use this checklist for every feature, route, page, migration, integration, and substantial documentation change. A blank or “not applicable” answer must include a reason.

## Ownership and access

- What product owns the business truth?
- What tenant owns each record, read model, cache key, job, export, and file?
- Is actor identity derived from validated context?
- What permission/service scope controls every action?
- Is the endpoint authorization matrix updated?
- Does the change rely on a removed product-entitlement concept?
- Is any Compliance Core UI/admin route platform-admin-only while runtime operation remains available to product workflows?

## Durability and workflow

- Is every advertised write durable across refresh, restart, and multiple replicas?
- Are fixtures, demo providers, no-op handlers, and local-success fallbacks unreachable in production?
- What state machine, concurrency token, idempotency record, transaction, outbox, and inbox behavior applies?
- What happens on duplicate, retry, conflict, partial failure, dependency failure, and recovery?

## Cross-product integrity

- Are foreign records referenced through owner APIs/events rather than copied or free-typed?
- Are snapshots labeled and limited to display/audit needs?
- Does quick create call the owning product and return to the originating workflow?
- Are event, handoff, API, and review-queue contracts updated and tested?

## Page and UI

- Which page constitution/archetype applies?
- Does the page use shared shell/header/actions/filters/table/form/drawer/dialog/state components?
- Is the layout readable without walls of text, giant forms, or overloaded tables?
- Are internal IDs/keys hidden outside explicit admin surfaces?
- Are loading, empty, no-results, forbidden, not-found, conflict, stale, degraded, partial, and error states implemented?
- Is success withheld until durable confirmation?
- Is user input preserved on recoverable failure?
- Is light/dark contrast proven for all states?
- Is responsive, keyboard, focus, screen-reader, and print behavior proven where applicable?

## Files, evidence, and audit

- Are upload size/type/signature/quarantine/scan/hash controls present?
- Are retention, legal hold, supersession, access, and purge rules enforced?
- Is audit attribution immutable and human-readable?

## Release proof

- What migration and rollback/supersession proof exists?
- What tenant-isolation, permission-denial, persistence, restart, concurrency, idempotency, contract, E2E, theme, accessibility, and route tests exist?
- Does clean CI discover this app/package and run real commands?
- Are docs indexes, product manifests, links, route maps, and event catalogs updated?
