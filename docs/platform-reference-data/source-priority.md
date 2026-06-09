# Platform Reference Data Source Priority

## Priority model

Source priority is used when multiple records describe the same reference entity.

Higher priority sources should win on canonical fields unless a reviewer overrides them.

## Suggested authority rank

1. Platform-admin curated canonical source
2. Direct owner-product import
3. Official public API connector
4. Vendor feed
5. CSV manual import
6. Manual single-record edit
7. Historical migration seed

## Practical rules

- If the source is authoritative and public, prefer it for canonical identity.
- If the source is a tenant overlay, preserve it as a tenant overlay rather than promoting it.
- If the source is a document file, store the file in RecordArr and import only the normalized metadata.
- If the source is a rule meaning, keep it in Compliance Core.

## Conflict resolution

- Exact identifier match: usually auto-link candidate.
- Multiple candidates with high confidence: hold for review.
- Published entity disagreement: require reviewer approval.
- Tenant-specific naming: keep as overlay, not canonical identity.
