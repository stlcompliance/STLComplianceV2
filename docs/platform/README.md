# STL Compliance Platform Planning Documents

These documents coordinate the existing product suite. They are version-neutral and do not define a second-generation architecture.

## Files

- `implementation-sequencing.md` — dependency-aware implementation order and release gates.
- `product-ui-route-map.md` — canonical route and navigation inventory across products.
- `event-catalog-and-consumer-matrix.md` — emitted facts, consumers, idempotency, and contract ownership.
- `integration-contracts-and-review-queues.md` — synchronous contracts, async review queues, and source-of-truth boundaries.
- `audit-packet-standards.md` — consistent audit-package structure and evidence provenance.
- `product-availability-and-access-model.md` — nonvariable product availability, local permissions, and Compliance Core UI restriction.
- `audit-remediation-acceptance-matrix.md` — audit findings converted into release acceptance evidence.
- `endpoint-authorization-map-template.md` — mandatory route/auth/tenant/actor/permission inventory.
- `production-readiness-scorecard-template.md` — per-product release decision checklist.
- `unified-ui-page-coverage-matrix.md` — primary-record/page-archetype coverage across every product.
- `unified-ui-regression-verification.md` — static, behavior, visual, accessibility, light/dark, and truthful-state regression checks.

The platform documents may add implementation detail but may not override the ownership or security constitutions.
