# A report is missing data

## Symptoms
- Report is empty or incomplete.
- Dashboard does not match source product.
- Export is missing expected records.

## Likely Causes
- Filters exclude data.
- Source product has not produced the record.
- Read model or dataset is stale.
- User lacks permission to view some data.

## What to Check
1. Review report filters.
2. Open the source product record.
3. Check ReportArr ingestion status or refresh jobs.
4. Check access policy.

## How to Fix
- Adjust filters.
- Correct source data in the owning product.
- Refresh dataset or read model where allowed.
- Request report access if needed.

## Who Can Help
ReportArr admin, report builder, or source product admin.

## Related Docs
- [How to generate a report](../how-to/reportarr/how-to-generate-a-report.md)
