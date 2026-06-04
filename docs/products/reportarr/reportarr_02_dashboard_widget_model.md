# ReportArr — Dashboard and Widget Model

## Dashboard

A Dashboard is an interactive reporting surface composed of widgets, filters, drilldowns, and access rules.

```text
Dashboard
- dashboardId
- tenantId
- dashboardNumber
- dashboardKey
- title
- description
- dashboardType
  - executive
  - product
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
  - audit
  - custom
- status
  - draft
  - active
  - paused
  - archived
- ownerPersonId
- defaultDateRange
- widgetRefs
- filterRefs
- drilldownRefs
- accessPolicyRef
- freshnessStatus
- lastViewedAt
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Dashboard status definitions

```text
draft
- Dashboard is being configured.

active
- Dashboard is available to permitted users.

paused
- Dashboard exists but should not be shown by default.

archived
- Dashboard is retained for history.
```

## Dashboard widget

```text
DashboardWidget
- widgetId
- tenantId
- dashboardId
- widgetKey
- title
- description
- widgetType
  - metric
  - chart
  - table
  - trend
  - exception_list
  - map
  - timeline
  - status_card
  - heatmap
  - gauge
  - markdown
  - drilldown_link
- status
  - active
  - hidden
  - failed
  - archived
- datasetRef
- readModelRef
- queryDefinition
- visualizationSettings
- filterBindings
- drilldownTargetRef
- sortOrder
- layout
- freshnessStatus
- lastRenderedAt
```

## Widget visualization settings

```text
WidgetVisualizationSettings
- widgetId
- chartType
  - line
  - bar
  - stacked_bar
  - area
  - pie
  - donut
  - table
  - number
  - gauge
  - heatmap
  - map
  - timeline
- xField
- yField
- seriesField
- groupField
- valueField
- labelField
- dateField
- colorRuleRefs
- thresholdRefs
- displayFormat
- showLegend
- showDataLabels
- maxRows
```

## Dashboard filter

```text
DashboardFilter
- filterId
- dashboardId
- filterKey
- label
- filterType
  - date_range
  - product
  - site
  - department
  - person
  - asset
  - customer
  - supplier
  - status
  - severity
  - category
  - custom
- datasetFieldKey
- defaultValue
- allowedValuesSource
- required
- visible
```

## Drilldown definition

Drilldowns let a user go from a report number to source details.

```text
DrilldownDefinition
- drilldownId
- tenantId
- title
- description
- sourceWidgetRef
- targetType
  - dashboard
  - report
  - product_object
  - record_package
  - external_url
- targetRef
- parameterMappings
- requiredPermissionRefs
- status
```

## Reporting alert

A ReportingAlert is triggered by a metric, threshold, stale data, or exception query.

```text
ReportingAlert
- alertId
- tenantId
- alertNumber
- title
- description
- alertType
  - threshold
  - exception
  - stale_data
  - failed_refresh
  - scheduled_report_failed
  - compliance_gap
  - overdue_action
- status
  - active
  - triggered
  - acknowledged
  - resolved
  - muted
  - archived
- datasetRef
- metricRef
- condition
- severity
  - low
  - moderate
  - high
  - critical
- triggeredAt
- acknowledgedByPersonId
- acknowledgedAt
- resolvedAt
- notificationRefs
```

## Dashboard access policy

```text
DashboardAccessPolicy
- accessPolicyId
- tenantId
- dashboardId
- visibility
  - private
  - role_based
  - product_based
  - tenant_wide
  - auditor
- allowedPersonRefs
- allowedRoleRefs
- allowedPermissionRefs
- sourceProductRestrictions
- exportAllowed
- createdAt
- updatedAt
```

## Dashboard families

## Executive dashboard

```text
- Compliance readiness
- Open critical blockers
- Asset downtime
- Work order aging
- Training overdue
- Inventory health
- Route on-time rate
- Order fulfillment performance
- Supplier quality
- Customer complaint trend
- CAPA aging
```

## Maintenance dashboard

```text
- Asset readiness
- Open work orders
- Emergency work
- Waiting parts
- PM compliance
- Defect severity
- Downtime
- Technician workload
```

## Inventory dashboard

```text
- Inventory value snapshot/reference
- Stockout risk
- On-hand/available/reserved
- Receiving cycle time
- Putaway backlog
- Pick performance
- Count variance
- Holds/quarantine
```

## Training/workforce dashboard

```text
- Training completion
- Overdue assignments
- Expiring qualifications
- People readiness
- Incident-driven retraining
- Qualification coverage by site/role
```

## Transportation dashboard

```text
- On-time departure
- On-time arrival
- Active trips
- Late stops
- Route exceptions
- Proof capture rate
- Breakdown delays
```

## Quality dashboard

```text
- Open NCRs
- Active holds
- CAPA aging
- Audit findings
- Supplier quality issues
- Customer complaints
- Repeat nonconformances
```

## Compliance dashboard

```text
- Rulepack status
- Missing evidence
- Invalid evidence
- Expiring evidence
- Noncompliant objects
- Manual review queue
- Audit readiness
```

## Dashboard workflow

```text
1. User creates dashboard or selects template.
2. User adds widgets.
3. Widgets are bound to datasets/read models.
4. Filters and drilldowns are configured.
5. Access policy is configured.
6. Dashboard becomes active.
7. Users view dashboard.
8. Freshness and drilldown links remain visible.
```

## Widget render workflow

```text
1. User opens dashboard.
2. ReportArr validates access.
3. ReportArr resolves widget datasets.
4. QueryDefinition runs against read model.
5. Widget renders metric/chart/table/list.
6. Widget includes freshness/source metadata.
7. User can drill into source object if allowed.
```

## Events

```text
reportarr.dashboard.created
reportarr.dashboard.updated
reportarr.dashboard.activated
reportarr.dashboard.archived
reportarr.dashboard.viewed

reportarr.widget.created
reportarr.widget.updated
reportarr.widget.rendered
reportarr.widget.render_failed

reportarr.filter.created
reportarr.drilldown.created
reportarr.drilldown.used

reportarr.alert.created
reportarr.alert.triggered
reportarr.alert.acknowledged
reportarr.alert.resolved
reportarr.alert.muted
```
