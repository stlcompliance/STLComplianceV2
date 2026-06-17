# How to place or release a quality hold

## Audience
Quality reviewers, quality managers, warehouse supervisors, maintenance supervisors, and release approvers

## Product
AssurArr

## Support Status
Supported by current UI/API

## Purpose
Block or release affected work, inventory, assets, orders, shipments, documents, or suppliers when a quality decision is required.

## Before You Start
- AssurArr owns the quality hold and release decision.
- The affected product owns the blocked execution record and must obey the hold signal.
- RecordArr owns release evidence and supporting documents.

## Steps
1. Open AssurArr.
2. Open Holds.
3. Select Create hold.
4. Enter hold title, severity, hold type, hold scope, and reason.
5. Link the source nonconformance or source object when known.
6. Add affected objects, quantity, lot, serial, site, location, owner, expiration, and release requirements when available.
7. Create the hold and confirm the status is active when it should block work.
8. When the issue is ready for review, open the hold detail or Releases.
9. Request release, attach evidence, and record conditions or notes.
10. Approve, reject, or execute the release based on release requirements and evidence.

## What Happens Next
AssurArr publishes hold and release status. LoadArr, MaintainArr, OrdArr, RoutArr, SupplyArr, RecordArr, StaffArr, or TrainArr respond only for records they own.

## Troubleshooting
- If an affected product still allows blocked work, check the hold status, target reference, and product integration timing.
- If release cannot be approved, verify required evidence and release authority.
- If physical inventory must move, complete the movement in LoadArr after the AssurArr decision.

## Related How-To Documents
- [How to create a nonconformance](create-a-nonconformance.md)
- [How to review audit-ready records](../recordarr/review-audit-ready-records.md)

