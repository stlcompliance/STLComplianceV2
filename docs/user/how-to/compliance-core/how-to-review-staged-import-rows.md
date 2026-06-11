# How to review staged import rows

## Audience
Compliance admins and reviewers.

## Purpose
Review imported rows before committing them to compliance reference data.

## Before You Start
- Compliance Core access.
- Import read access.
- An import batch with staged rows.

## Steps
1. Open Compliance Core.
2. Open **Imports**.
3. Select the import batch.
4. Review staged rows, validation status, confidence, and required mapping fields.
5. Map rows where mapping is required.
6. Reject rows that should not be committed when the action is available.
7. Ask a compliance admin to commit only after review is complete.

## What Happens Next
Reviewed rows can be mapped and committed by an authorized user. Unreviewed or rejected rows should not become active reference data.

## Troubleshooting
- If rows are missing, check the selected import batch.
- If mapping is disabled, check compliancecore.import.map access.
- If commit is unavailable, check compliance admin or platform admin access.

## Related Docs
- [How to map uploaded documents to requirements](how-to-map-uploaded-documents-to-requirements.md)
- [Common statuses](../../reference/common-statuses.md)
