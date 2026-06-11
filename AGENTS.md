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

## Deployment Rule

- This project is deployed on Render.
- Local-only changes do not change remote or deployed status.
- Render deploys are automatically triggered only after code is committed and pushed.
