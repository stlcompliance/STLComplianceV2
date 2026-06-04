# ReportArr — Report Definition, Run, Schedule, and Export Model

## Report definition

A ReportDefinition defines a reusable report with parameters, datasets, layout, access policy, and export formats.

```text
ReportDefinition
- reportDefinitionId
- tenantId
- reportNumber
- reportKey
- title
- description
- reportType
  - operational
  - compliance
  - audit
  - executive
  - exception
  - scheduled
  - management_review
  - customer
  - supplier
  - product
  - custom
- status
  - draft
  - active
  - paused
  - archived
- datasetRefs
- readModelRefs
- parameterRefs
- defaultFilters
- layoutDefinition
- sectionRefs
- exportFormats
  - pdf
  - csv
  - xlsx
  - json
  - html
  - zip
- accessPolicyRef
- ownerPersonId
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Report parameter

```text
ReportParameter
- parameterId
- reportDefinitionId
- parameterKey
- label
- parameterType
  - date_range
  - single_select
  - multi_select
  - text
  - number
  - boolean
  - person
  - site
  - location
  - asset
  - customer
  - supplier
  - product
  - status
  - severity
- required
- defaultValue
- allowedValuesSource
- validationRules
```

## Report section

```text
ReportSection
- sectionId
- reportDefinitionId
- sequence
- title
- description
- sectionType
  - summary
  - table
  - chart
  - metric_grid
  - narrative
  - exception_list
  - evidence_matrix
  - source_trace
  - appendix
- datasetRef
- queryDefinition
- layoutSettings
```

## Report run

A ReportRun is one execution of a report definition.

```text
ReportRun
- reportRunId
- tenantId
- reportRunNumber
- reportDefinitionId
- status
  - queued
  - running
  - completed
  - completed_with_warnings
  - failed
  - canceled
- requestedByPersonId
- requestedAt
- startedAt
- completedAt
- parametersUsed
- filtersUsed
- outputFormat
- outputRecordRef
- outputPackageRef
- rowCount
- warningCount
- errorCount
- errorMessage
- sourceTraceSummary
- freshnessSummary
```

## Report run status definitions

```text
queued
- Report is waiting to execute.

running
- Report generation is active.

completed
- Report generated successfully.

completed_with_warnings
- Report generated but data freshness, missing sources, or partial issues exist.

failed
- Report did not generate.

canceled
- Report run was canceled.
```

## Report schedule

```text
ReportSchedule
- scheduleId
- tenantId
- reportDefinitionId
- title
- status
  - active
  - paused
  - canceled
  - expired
- cadence
  - hourly
  - daily
  - weekly
  - monthly
  - quarterly
  - annually
  - custom_cron
- timezone
- cronExpression
- nextRunAt
- lastRunAt
- startsAt
- endsAt
- parameters
- recipients
- deliveryMethod
  - email
  - recordarr_package
  - dashboard_notification
  - webhook
  - download_only
- createdByPersonId
- createdAt
- updatedAt
```

## Report recipient

```text
ReportRecipient
- recipientId
- scheduleId
- recipientType
  - person
  - email
  - role
  - team
  - external
  - webhook
- recipientRef
- email
- deliveryFormat
  - pdf
  - csv
  - xlsx
  - link
  - package
- status
  - active
  - inactive
```

## Export job

An ExportJob is a file-generation job that may be tied to a report, dashboard, table, audit package, or raw dataset.

```text
ExportJob
- exportJobId
- tenantId
- exportNumber
- exportType
  - report
  - dashboard
  - table
  - dataset
  - audit_package
  - chart
  - custom
- status
  - queued
  - running
  - completed
  - failed
  - canceled
  - expired
- requestedByPersonId
- requestedAt
- startedAt
- completedAt
- exportFormat
  - pdf
  - csv
  - xlsx
  - json
  - png
  - zip
- sourceRef
- outputRecordRef
- rowCount
- fileSizeBytesSnapshot
- expiresAt
- errorMessage
```

## Audit report package

An AuditReportPackage is a reporting-side audit view tied to Compliance Core and RecordArr evidence packages.

```text
AuditReportPackage
- auditReportPackageId
- tenantId
- packageNumber
- title
- description
- auditScope
- status
  - draft
  - assembling
  - complete
  - locked
  - archived
  - failed
- requestedByPersonId
- complianceEvaluationRefs
- sourceProductRefs
- sourceObjectRefs
- recordarrPackageRef
- reportRunRefs
- missingEvidenceSummary
- invalidEvidenceSummary
- readinessScore
- generatedAt
- lockedAt
```

## Audit scope

```text
AuditScope
- auditScopeId
- tenantId
- scopeType
  - rulepack
  - site
  - department
  - person
  - asset
  - vehicle
  - supplier
  - customer
  - order
  - date_range
  - incident
  - quality
  - custom
- dateRangeStart
- dateRangeEnd
- productFilters
- objectRefs
- rulepackRefs
- siteRefs
- departmentRefs
- includeEvidence
- includeSourceTrace
```

## Report access policy

```text
ReportAccessPolicy
- accessPolicyId
- tenantId
- reportDefinitionId
- visibility
  - private
  - role_based
  - permission_based
  - tenant_wide
  - auditor
- allowedPersonRefs
- allowedRoleRefs
- allowedPermissionRefs
- sourceProductRestrictions
- exportAllowed
- scheduleAllowed
- externalDeliveryAllowed
```

## Report run workflow

```text
1. User selects report definition.
2. ReportArr validates access.
3. User enters parameters.
4. ReportRun is queued.
5. ReportArr resolves datasets/read models.
6. Data freshness is checked.
7. Report sections render.
8. Export file is generated if requested.
9. Output is stored in RecordArr.
10. ReportRun completes or fails.
```

## Scheduled report workflow

```text
1. ReportSchedule reaches nextRunAt.
2. ReportArr creates ReportRun.
3. Report is generated.
4. Output file is stored in RecordArr.
5. Recipients are notified or delivery webhook is called.
6. Schedule calculates nextRunAt.
```

## Audit report workflow

```text
1. User selects audit scope.
2. ReportArr requests Compliance Core evaluations.
3. ReportArr requests evidence package state from RecordArr.
4. ReportArr aggregates source product facts.
5. AuditReportPackage is assembled.
6. RecordArr stores final package/export.
7. Package can be locked.
```

## Events

```text
reportarr.report_definition.created
reportarr.report_definition.updated
reportarr.report_definition.activated
reportarr.report_definition.archived

reportarr.report_run.queued
reportarr.report_run.started
reportarr.report_run.completed
reportarr.report_run.completed_with_warnings
reportarr.report_run.failed
reportarr.report_run.canceled

reportarr.schedule.created
reportarr.schedule.paused
reportarr.schedule.resumed
reportarr.schedule.canceled
reportarr.schedule.run_triggered

reportarr.export.queued
reportarr.export.started
reportarr.export.completed
reportarr.export.failed
reportarr.export.expired

reportarr.audit_package.created
reportarr.audit_package.assembling
reportarr.audit_package.completed
reportarr.audit_package.locked
reportarr.audit_package.failed
```
