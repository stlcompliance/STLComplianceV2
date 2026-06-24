# Render Staging Ship Gate

Use the staging ship gate to validate the Render blueprint inventory before promoting changes.

## Validation

- `scripts/ops/render-staging-ship-gate-validate.ps1`
- `scripts/ops/render-staging-ship-gate-validate.sh`

## What it checks

- Render blueprint inventory matches the catalog.
- Required API probes and static-site probes are present.
- Staging gating metadata stays aligned with the repo catalogs.

## Notes

- Keep this runbook aligned with the ship-gate catalog and validation scripts.
- Update the catalog first when adding or renaming products, services, or probes.
