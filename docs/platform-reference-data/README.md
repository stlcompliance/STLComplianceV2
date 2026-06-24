# Platform Reference Data Service

This directory defines a narrow platform capability for curated public identifiers, approved reusable taxonomies, units of measure, aliases, and crosswalks. It is not a user-facing product and must not become a catch-all source of truth merely because multiple products reference a concept.

Use an owning product for people, locations, customers, suppliers, items/SKUs, assets, records, rules, and operational facts. Use this service only when identity/normalization genuinely exists independently of tenant workflow and has been explicitly approved as platform reference data.

## Documents

- `architecture.md`
- `product-boundaries.md`
- `target-inventory.md`
- `source-priority.md`
- `import-process.md`
- `api.md`
- `public-identifier-model.md`
- `uom-package-normalization-model.md`
- `manufacturer-brand-taxonomy-model.md`
- `crosswalk-alias-resolution-model.md`
- `workflows-status-events-apis.md`
- `open-questions.md`

Administration is a platform-admin surface. Published lookup/normalization APIs are consumed by products through service identity and tenant-aware contracts where tenant overlays exist.
