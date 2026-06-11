# How to import rule reference data

## Audience
Compliance admins and rulepack managers

## Product
Compliance Core

## Support Status
Supported by current UI/API

## Purpose
Bring governing bodies, citations, requirements, or other reference data into Compliance Core for review.

## Before You Start
- Compliance Core owns governing bodies, citations, rulepacks, vocabulary, applicability logic, evidence requirements, exemptions, exceptions, and audit logic.
- Do not paste raw technical payloads into user-facing notes.

## Steps
1. Open Compliance Core.
2. Open Imports.
3. Choose the import workflow provided for the data type.
4. Select the source file or configured source.
5. Map columns or fields using the import UI.
6. Run validation or staging.
7. Review staged import rows before accepting changes.
8. Import only when validation and ownership look correct.
9. Review Registry or Mappings after import.

## What Happens Next
Imported data becomes Compliance Core reference or rule data after review. Operational products still own their execution records.

## Troubleshooting
- If rows fail validation, fix the source data or mapping and stage the import again.
- If the import would duplicate existing governing bodies or citations, resolve the duplicate before accepting.

## Related How-To Documents
- [How to review staged import rows](../compliancecore/review-staged-import-rows.md)

