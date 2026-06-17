# How to create a nonconformance

## Audience
Quality technicians, quality reviewers, managers, and authorized operations users

## Product
AssurArr

## Support Status
Supported by current UI/API

## Purpose
Open an AssurArr quality case when work, inventory, documents, supplier performance, customer impact, or process execution does not meet requirements.

## Before You Start
- AssurArr owns nonconformance truth, classification, quality workflow state, and closure.
- The source product still owns the operational record that revealed the issue.
- RecordArr owns stored evidence files.
- StaffArr owns people and internal location references.

## Steps
1. Open AssurArr.
2. Open Nonconformances.
3. Select Create nonconformance.
4. Enter a clear title and description.
5. Choose severity, type, category, source product, and source object reference.
6. Add affected objects, owner, site, location, due date, and recurrence details when available.
7. Link evidence records or add notes for follow-up evidence capture.
8. Create the nonconformance.
9. Open the detail page and review source context, holds, containment, disposition, CAPA, findings, evidence, and timeline.

## What Happens Next
AssurArr tracks the quality case and emits quality events. Holds, containment, disposition, CAPA, and release decisions stay in AssurArr, while execution work remains in the owning product.

## Troubleshooting
- If the source object is wrong, correct it in the product that owns the source record.
- If evidence is missing, upload or attach it through RecordArr-backed capture.
- If the issue blocks inventory, assets, shipments, or orders, place a quality hold instead of relying on notes.

## Related How-To Documents
- [How to place or release a quality hold](place-or-release-a-quality-hold.md)
- [How to create and close a CAPA](create-and-close-a-capa.md)

