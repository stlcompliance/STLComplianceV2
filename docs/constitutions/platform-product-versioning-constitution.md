# STL Compliance Product Versioning Constitution

## 1. Purpose

This constitution defines how touched products advance their version number in STL Compliance.

## 2. Scope

This constitution applies to product code, product manifests, and release-facing version metadata for STL Compliance products.

## 3. Prime directive

Touched products advance by one version step per commit. The version is derived from repository history, not hand-edited arbitrarily.

## 4. Canonical format

The canonical product version format is:

```text
0.8.<sequence>
```

- `0.8.582` is the baseline start point for this rule.
- The trailing three digits are the repository commit sequence number for the commit being prepared.
- Tooling should use the next commit sequence number, not the current `HEAD` value, so the committed change and the version line up.
- If one commit touches multiple products, each touched product uses the same sequence number.

## 5. Update rule

If a commit touches a product, every version-bearing manifest for that product must be updated in the same commit.

If a commit does not touch a product, that product's version must not change.

Shared packages, shared libraries, and repository-wide tooling only change version when the shared component itself is touched.

## 6. Automation rule

Version changes must be produced by tooling that derives the sequence from git history.

Manual version edits are not the source of truth.

The tooling must treat the current repository state as the baseline and compute the next commit sequence for the version bump.

## 7. Exceptions

- Documentation-only changes outside a product manifest do not require a product version bump.
- Generated lockfiles and generated metadata follow the version of the manifest they belong to.
- If a product has multiple version-bearing manifests, they all move together.

