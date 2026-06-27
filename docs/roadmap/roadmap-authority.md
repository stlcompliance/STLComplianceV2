# Roadmap Authority and Non-Loss Rule

## Purpose

The roadmap layer controls sequencing. It does not reduce the product universe.

## Authority order

When deciding whether work belongs in the current release, use this order:

1. Platform and ownership constitutions.
2. R0 trust gate requirements.
3. Release-stage scope in this roadmap directory.
4. Cross-product vertical slice acceptance criteria.
5. Product `FEATURESET.md` and `WORKFLOWS.md` catalogs.
6. User guidance and implementation notes.

When deciding whether a feature belongs in the product at all, use the product feature and workflow catalogs. The roadmap can defer a feature; it cannot silently delete it.

## Non-loss rule

A feature or workflow may move between releases only if the move is explicit and the row remains in the inventory. A release cut must never use scope control as an excuse to remove ownership boundaries, event discipline, tenant scope, permission enforcement, error truthfulness, light/dark readability, print/report expectations, quick create, or cross-product reference correctness.

## Done means production truth, not screen presence

A release is not complete because pages render. A release is complete when the owning product can prove:

- durable persistence and restart survival;
- tenant isolation and server-side authorization;
- idempotent unsafe actions and safe retries;
- source-of-truth boundaries and no local shadow owners;
- clear status transitions, blockers, and degraded states;
- evidence/reference capture through the owning product or RecordArr;
- accessible, readable, unified light/dark UI;
- professional print/export behavior where relevant;
- source refs, events, audit trail, and reportability.

## Pull-forward rule

A later-stage feature can be pulled forward only when all of these are true:

1. It is required to complete the current vertical slice.
2. Its source owner is already present or is explicitly delivered as part of the same slice.
3. It does not create temporary shadow truth in another product.
4. It passes the same release gates as same-stage features.
5. The pull-forward is documented in the release notes and CSV inventory.
