# How to review a supplier quality issue

## Audience
Supplier quality managers, supply chain users, quality reviewers, and purchasing managers

## Product
AssurArr

## Support Status
Supported by current UI/API

## Purpose
Track a supplier-facing quality issue, supplier corrective action request, and supplier response without making AssurArr own the vendor master.

## Before You Start
- AssurArr owns supplier quality issue workflow, quality status, and supplier corrective action request review.
- SupplyArr owns vendor and supplier master context.
- RecordArr owns supplier response files and evidence records.

## Steps
1. Open AssurArr.
2. Open Supplier quality.
3. Create or select the supplier quality issue.
4. Review issue type, supplier reference, affected receipt, affected purchase order, affected item, nonconformance, hold refs, and evidence.
5. If supplier response is required, open SCARs and create a supplier corrective action request.
6. Set supplier due date, source nonconformance or CAPA reference, and requested evidence.
7. Attach supplier response records when received.
8. Review the response and mark the SCAR accepted, rejected, closed, or requiring follow-up according to the visible workflow.
9. Coordinate supplier master or purchasing impact in SupplyArr.

## What Happens Next
AssurArr records supplier quality decisions and status. SupplyArr consumes supplier quality context while retaining ownership of supplier master and procurement records.

## Troubleshooting
- If the supplier is missing, correct the supplier record in SupplyArr.
- If response evidence is missing, upload the file through RecordArr-backed capture.
- If the supplier should be blocked from future work, update the AssurArr quality status and coordinate the SupplyArr supplier status path.

## Related How-To Documents
- [How to create a vendor](../supplyarr/create-a-vendor.md)
- [How to review pricing and lead-time snapshots](../supplyarr/review-pricing-and-lead-time-snapshots.md)

