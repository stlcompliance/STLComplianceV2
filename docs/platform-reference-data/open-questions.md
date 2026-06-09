# Platform Reference Data Open Questions

## Questions to resolve

- Which existing product tables already contain reusable vehicle or GTIN identity that should be migrated?
- Which equipment taxonomies are truly shared versus tenant-specific?
- Should SDS metadata ingest be triggered by RecordArr upload, SupplyArr item creation, or both?
- Which connector sources are considered authoritative for manufacturer and product identity?
- Do we want one universal public-product catalog or separate dataset keys for vehicle, product, chemical, and equipment identity?
- Should tenant overlays support aliases only, or also tenant-local status labels and visibility flags?
- Which review actions should require a platform owner versus a platform admin?
- What is the final set of service-token scopes and their naming convention?

## Current working assumptions

- NexArr hosts the platform control plane.
- ReferenceDataCore is a platform-owned service, not a separate product database joined directly by products.
- RecordArr keeps document truth.
- Compliance Core keeps regulatory meaning.
- SupplyArr keeps commercial context.
- MaintainArr keeps asset truth.

## Decision notes

When a question remains open, the default is to centralize only the shared identity layer and leave commercial, operational, and legal interpretation local to the owning product.
