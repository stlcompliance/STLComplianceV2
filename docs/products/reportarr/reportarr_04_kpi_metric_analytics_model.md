# ReportArr — KPI, Metric, Analytics, and Alert Model

## KPI definition

A KPI definition is a named performance indicator with formula, thresholds, source datasets, and display rules.

```text
KpiDefinition
- kpiId
- tenantId
- kpiNumber
- kpiKey
- title
- description
- category
  - compliance
  - maintenance
  - training
  - workforce
  - inventory
  - procurement
  - transportation
  - customer
  - order
  - quality
  - mobile
  - platform
  - executive
- status
  - draft
  - active
  - paused
  - archived
- formula
- sourceDatasetRefs
- sourceMetricRefs
- targetValue
- warningThreshold
- criticalThreshold
- higherIsBetter
- displayFormat
  - number
  - percentage
  - duration
  - currency_reference
  - count
  - rate
  - score
- ownerPersonId
- createdAt
- updatedAt
```

## Metric definition

A MetricDefinition is a reusable calculation that can support KPIs, dashboards, and reports.

```text
MetricDefinition
- metricId
- tenantId
- metricKey
- title
- description
- metricType
  - count
  - sum
  - average
  - median
  - percentage
  - ratio
  - duration
  - aging
  - trend
  - score
- sourceDatasetRef
- fieldRefs
- formula
- filterDefinition
- groupingOptions
- dateField
- status
```

## KPI value

```text
KpiValue
- kpiValueId
- tenantId
- kpiId
- periodStart
- periodEnd
- value
- targetValueSnapshot
- warningThresholdSnapshot
- criticalThresholdSnapshot
- status
  - good
  - warning
  - critical
  - unknown
- trend
  - improving
  - stable
  - worsening
  - unknown
- sourceTraceSummary
- calculatedAt
```

## Metric value

```text
MetricValue
- metricValueId
- tenantId
- metricId
- periodStart
- periodEnd
- value
- groupKey
- groupLabel
- sourceTraceSummary
- calculatedAt
```

## Analytics snapshot

An AnalyticsSnapshot freezes a point-in-time view of metrics/read models for trend and audit repeatability.

```text
AnalyticsSnapshot
- analyticsSnapshotId
- tenantId
- snapshotNumber
- snapshotType
  - daily
  - weekly
  - monthly
  - quarterly
  - audit
  - manual
- status
  - created
  - completed
  - failed
  - archived
- periodStart
- periodEnd
- datasetRefs
- kpiValueRefs
- metricValueRefs
- generatedAt
- generatedBy
  - system
  - person
```

## Trend analysis

```text
TrendAnalysis
- trendAnalysisId
- tenantId
- metricRef
- kpiRef
- periodStart
- periodEnd
- trend
  - improving
  - stable
  - worsening
  - volatile
  - unknown
- changeValue
- changePercent
- confidence
  - low
  - medium
  - high
- explanation
- generatedAt
```

## Exception query

Exception queries identify records needing attention.

```text
ExceptionQuery
- exceptionQueryId
- tenantId
- queryKey
- title
- description
- sourceDatasetRef
- condition
- severity
  - low
  - moderate
  - high
  - critical
- status
  - active
  - paused
  - archived
- ownerPersonId
```

## Exception result

```text
ExceptionResult
- exceptionResultId
- exceptionQueryId
- tenantId
- sourceObjectRef
- title
- summary
- severity
- status
  - open
  - acknowledged
  - resolved
  - dismissed
- detectedAt
- acknowledgedByPersonId
- acknowledgedAt
- resolvedAt
- sourceTrace
```

## Common KPI families

## Platform and access

```text
- Login failure rate
- Product launch denial count
- Active tenant count
- Entitlement suspension count
- Service token failure count
- Suspicious activity count
```

## Staff/workforce

```text
- Active headcount
- People readiness rate
- Permission review overdue count
- Personnel incident count
- Active restriction count
- Open corrective action count
```

## Training

```text
- Training completion rate
- Overdue assignment count
- Expiring qualification count
- Expired qualification count
- Remediation assignment count
- Qualification coverage by role/site
```

## Compliance

```text
- Compliance status by rulepack
- Missing evidence count
- Invalid evidence count
- Expiring evidence count
- Manual review queue count
- Audit readiness score
```

## Maintenance

```text
- Asset readiness rate
- Open work order count
- Work order aging
- Waiting parts count
- PM compliance rate
- Overdue PM count
- Defect count by severity
- Asset downtime hours
- Repeat defect rate
```

## Inventory/WMS

```text
- Inventory accuracy rate
- Count variance value/quantity
- Stockout count
- Reservation fill rate
- Receiving cycle time
- Putaway cycle time
- Pick accuracy
- Held/quarantined quantity
```

## Procurement

```text
- Purchase request aging
- PO cycle time
- Supplier on-time rate
- Supplier quality issue count
- Average lead time
- Emergency purchase count
```

## Transportation

```text
- On-time departure rate
- On-time arrival rate
- Active late stops
- Route exception count
- Proof capture rate
- Breakdown delay count
```

## Customer/order

```text
- Order fulfillment rate
- Order blocker count
- Customer SLA performance
- Customer complaint count
- Customer issue aging
- On-time delivery by customer
```

## Quality

```text
- Open NCR count
- Active hold count
- Hold aging
- CAPA aging
- Overdue CAPA count
- Ineffective CAPA count
- Audit finding count
- Repeat nonconformance rate
```

## Mobile execution

```text
- Mobile task completion rate
- Offline action count
- Sync failure count
- Capture completion rate
- Secure upload completion rate
- Field task aging
```

## KPI calculation workflow

```text
1. Dataset/read model refresh completes.
2. MetricDefinition calculation runs.
3. MetricValue records are created.
4. KpiDefinition formula runs.
5. KpiValue is created.
6. Threshold status is assigned.
7. TrendAnalysis may run.
8. Dashboards/reports update.
```

## Exception workflow

```text
1. ExceptionQuery runs.
2. Matching source objects create ExceptionResults.
3. ReportingAlert may trigger.
4. User acknowledges or resolves.
5. Source product remains owner of actual correction.
```

## Events

```text
reportarr.kpi_definition.created
reportarr.kpi_definition.updated
reportarr.kpi_value.calculated
reportarr.metric_definition.created
reportarr.metric_value.calculated
reportarr.analytics_snapshot.created
reportarr.analytics_snapshot.completed
reportarr.trend_analysis.completed
reportarr.exception_query.created
reportarr.exception_result.created
reportarr.exception_result.acknowledged
reportarr.exception_result.resolved
```
