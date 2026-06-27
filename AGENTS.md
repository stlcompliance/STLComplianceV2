# STLComplianceV2 Agent Rules

## Constitution-First Rule

All work in this repository must abide by the constitution documents under [`docs/constitutions/`](docs/constitutions/).

Before changing code, copy, data flow, ownership boundaries, or UI behavior:

1. Read the applicable constitution document(s).
2. Treat the constitutions as authoritative when they conflict with older implementation patterns or stale notes.
3. Prefer the most specific applicable constitution.

## Constitution Order of Authority

Use the most specific governing document available:

1. Product or page-specific constitutions
2. Shared UI/page constitutions
3. Ownership constitution

Key references:

- [`docs/constitutions/ownership.md`](docs/constitutions/ownership.md)
- [`docs/constitutions/ui.md`](docs/constitutions/ui.md)
- [`docs/constitutions/pages/`](docs/constitutions/pages/)

## Non-Negotiable Expectations

- Do not make a product the source of truth for another product's domain unless the constitution explicitly allows it.
- Do not introduce UI patterns that violate the shared shell or page constitutions.
- Do not duplicate canonical cross-product data unless the applicable constitution explicitly permits a labeled snapshot.
- If the request appears to conflict with a constitution, pause and call out the conflict instead of silently implementing it.

## Current-Slice Cross-Reference Enrichment Rule

When working on STL Compliance code, always look for reasonable cross-product references that could enrich product crosstalk, reduce duplicate entry, improve user context, or prevent operational mistakes.

This rule applies only to the current code slice being touched.

Do not perform a full repo scan every time this rule is invoked. Do not expand into unrelated products, routes, pages, schemas, services, tests, or docs unless the current task explicitly asks for a repo-wide audit, cross-product pass, or architecture review.

"Current code slice" means the files, components, routes, API handlers, schemas, services, tests, seed data, docs, or workflows already being edited or directly required to complete the requested change.

While working inside that slice, ask:

1. Does the record, page, form, drawer, workflow, event, import, report, or API being touched reference another product's owned data?
2. Would a small, relevant cross-reference help the user understand the current task?
3. Would it prevent duplicate work, missed handoffs, compliance gaps, bad references, or operational confusion?
4. Is the relationship actionable or useful right now, rather than merely technically connected?
5. Can the improvement be made within the current slice without turning the task into a broad refactor?

Allowed bounded discovery:

- Inspect files already opened for the task.
- Inspect directly imported components, types, hooks, API clients, validators, or tests.
- Inspect nearby route/page/service files when they are necessary to understand the current workflow.
- Inspect product docs or constitutions only when the touched slice already depends on them or the task asks for doc alignment.
- Follow obvious references one step outward when needed to avoid breaking existing behavior.

Disallowed behavior:

- Do not scan the whole repo looking for every possible cross-reference.
- Do not create broad cross-product panels just because data can be connected.
- Do not add unrelated product features while touching a narrow slice.
- Do not introduce cross-product database coupling.
- Do not duplicate another product's ownership.
- Do not expose internal IDs, product keys, entitlement language, tenant plumbing, or implementation details to normal users.
- Do not clutter pages with low-value history, noisy event streams, or excessive related records.

Preferred behavior:

- Add concise, high-signal related context only where it improves the current user workflow.
- Prefer compact badges, related-record cards, side-drawer sections, linked-record rows, timeline highlights, quick actions, and "view more" affordances.
- Keep default views focused; place deeper cross-product information behind tabs, drawers, expanders, or drill-in navigation.
- Prioritize active, blocking, overdue, exception, safety, compliance, audit, certification, assignment, location, inventory, customer, vendor, order, asset, document, or training relationships.
- Respect product ownership boundaries and use existing APIs, events, service contracts, or delegated actions.
- Preserve unified UI conventions, professional copy, light/dark readability, and current product layout patterns.
- When a potentially useful cross-reference is outside the current slice, leave it as a bounded TODO, note, or follow-up recommendation instead of expanding the task.

Cross-reference priority order:

1. Safety, compliance, legal, audit, certification, or permission impact.
2. Blocking operational dependency.
3. Current exception, overdue item, failed validation, or missing required reference.
4. Active linked work, order, shipment, asset, person, customer, vendor, inventory, document, location, or training item.
5. Recently changed or high-confidence related context.
6. Historical or informational links only when intentionally expanded by the user.

Acceptance standard:

A cross-reference is correct when it helps the user complete the current task, understand the current record, or avoid a real operational mistake. A cross-reference is incorrect when it exists only because the data can technically be joined, or when implementing it requires scanning or refactoring outside the current code slice without explicit instruction.

## Deployment Rule

- This project is deployed on Render.
- Local-only changes do not change remote or deployed status.
- Render deploys are automatically triggered only after code is committed and pushed.
