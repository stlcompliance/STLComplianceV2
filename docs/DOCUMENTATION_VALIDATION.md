# Documentation Validation

Documentation changes are release-relevant and must be checked like code.

## Required automated checks

1. All relative Markdown links resolve.
2. Every product manifest lists every canonical product Markdown file and no removed file.
3. No `platform/v2`, `workflows/v2`, version-addendum tree, or version-suffixed documentation filename exists. API versions such as `/api/v1/` are protocol versions, not documentation layers.
4. Current product, platform, how-to, and user guidance contains no product-entitlement entity, endpoint, event, invitation type, permission key, launch check, or tenant/user product grant. Explicit removal rules and historical audit evidence may name those retired concepts only to prohibit or document them.
5. `ReferenceDataCore` is absent from current architecture. Platform Reference Data is a narrow shared service and platform-admin utility, never a launcher product or catch-all owner.
6. Every product route in the canonical route map has an owner and page archetype.
7. Every endpoint family has authorization-map, permission, tenant-scope, actor-attribution, and contract-test requirements.
8. Every primary record names list, drawer, detail, create/edit or guided action, history, evidence, print/report, and designed state behavior where applicable.
9. Every page constitution requires shared primitives, central semantic tokens, equal light/dark readability, designed failure/degraded states, and regression proof.
10. Historical audit snapshots carry a superseding-context banner when old terminology is retained as evidence.

## Suggested repository checks

```text
python scripts/check-doc-links.py docs
python scripts/check-product-manifests.py docs/products
find docs -type d \( -iname v1 -o -iname v2 \)
find docs -type f | grep -Ei '(^|[/_.-])(v1|v2)([/_.-]|$)'
rg -n -i "ProductEntitlement|ProductAccessGrant|product_access_missing|nexarr\.product_access|defaultProductAccess" docs --glob '*.md' --glob '!audit/**'
rg -n -i "ReferenceDataCore|referencedatacore" docs --glob '*.md' --glob '!DOCS_MERGE_CHANGELOG.md' --glob '!DOCUMENTATION_VALIDATION.md'
```

Search results in the canonical removal constitution, merge changelog, validation instructions, or bannered historical audit files are not active-model violations. Review every other match.

## Required review evidence

A documentation change is ready only when the pull request identifies:

- the governing constitution or page archetype;
- ownership and cross-product boundary effects;
- access, permission, tenant, and Compliance Core studio/runtime effects;
- affected routes, endpoints, events, settings, reports, and user guidance;
- renamed or removed files and their replacement destinations;
- link, manifest, terminology, and page-coverage check results.
