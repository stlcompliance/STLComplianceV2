# How to review staged import rows

## Audience
Compliance admins and data stewards

## Product
Compliance Core

## Support Status
Supported by current UI/API

## Purpose
Check import rows before they become compliance reference data.

## Before You Start
- The import must already be staged.
- You need permission to review or approve Compliance Core imports.

## Steps
1. Open Compliance Core.
2. Open Imports.
3. Select the staged import.
4. Review row counts, validation messages, duplicates, and proposed changes.
5. Filter to errors or warnings first.
6. Correct mapping or source data if the row does not match the intended compliance object.
7. Approve or accept only rows that are safe to import.
8. Reject, hold, or restage rows that need more review.
9. After acceptance, review the target Registry or Mappings view.

## What Happens Next
Compliance Core updates its reference data only after the staged rows are accepted or imported.

## Troubleshooting
- If the UI does not expose row-level approval for the import type, keep the import staged and record the missing review workflow as a product gap.
- If a row changes rule meaning, involve the compliance owner before importing.

