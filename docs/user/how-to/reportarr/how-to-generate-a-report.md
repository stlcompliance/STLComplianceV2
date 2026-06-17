# How to generate a report

## Audience
ReportArr users with report run access.

## Purpose
Run an existing ReportArr report definition.

## Before You Start
- ReportArr access.
- Report run access such as report_runner, reportarr_runner, report_builder, reportarr_admin, or tenant_admin.
- An existing report definition.

![ReportArr report run screen showing report definition, date range, output format, and previous runs.](/screenshots/reportarr-report-run.png "Review required parameters before creating the report run, then open the output when it is ready.")

## Steps
1. Open ReportArr.
2. Open **Reports**.
3. Select the report definition.
4. Review required parameters or filters.
5. Choose export format if offered.
6. Create the report run.
7. Open the report run to review status and output.

## What Happens Next
ReportArr creates a report run. If retained as a record, RecordArr owns the stored artifact.

## Troubleshooting
- If run action is missing, check report run permission.
- If report data is missing, check source product data and read model freshness.
- If export fails, retry or ask an admin to review the export job.

## Related Docs
- [Report missing data](../../troubleshooting/report-missing-data.md)
- [ReportArr guide](../../products/reportarr-user-guide.md)
