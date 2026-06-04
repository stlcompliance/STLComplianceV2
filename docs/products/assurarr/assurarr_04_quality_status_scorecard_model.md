# AssurArr — Quality Status, Scorecard, and Metrics Model

## Quality status snapshot

QualityStatusSnapshot allows AssurArr to publish current quality state for an object owned elsewhere.

Examples:

- Supplier quality status for SupplyArr
- Inventory quality status for LoadArr
- Asset quality status for MaintainArr
- Order quality status for OrdArr
- Customer complaint status for CustomArr
- Document quality status for RecordArr

```text
QualityStatusSnapshot
- qualityStatusSnapshotId
- tenantId
- targetProduct
- targetObjectRef
- qualityStatus
  - acceptable
  - warning
  - on_hold
  - rejected
  - conditional_release
  - under_review
  - unknown
- severity
  - none
  - low
  - moderate
  - high
  - critical
- activeHoldRefs
- openNonconformanceRefs
- openCapaRefs
- openFindingRefs
- lastReviewedAt
- reviewedByPersonId
- expiresAt
- notes
```

## Quality scorecard

QualityScorecard summarizes quality performance for a supplier, customer, process, site, department, asset class, or other target.

```text
QualityScorecard
- scorecardId
- tenantId
- scorecardNumber
- targetType
  - supplier
  - customer
  - site
  - department
  - process
  - asset_class
  - inventory_item
  - product_service
  - route_lane
  - other
- targetRef
- periodStart
- periodEnd
- status
  - draft
  - active
  - finalized
  - archived
- overallScore
- qualityStatus
  - excellent
  - acceptable
  - warning
  - poor
  - blocked
  - unknown
- metricRefs
- trend
  - improving
  - stable
  - worsening
  - unknown
- generatedAt
- generatedBy
  - system
  - person
- reviewedByPersonId
- reviewedAt
```

## Quality metric

```text
QualityMetric
- metricId
- scorecardId
- metricKey
- title
- description
- category
  - nonconformance
  - hold
  - capa
  - audit
  - supplier
  - customer
  - delivery
  - inventory
  - maintenance
  - documentation
- value
- numerator
- denominator
- unit
- targetValue
- warningThreshold
- criticalThreshold
- status
  - good
  - warning
  - critical
  - unknown
- sourceProductRefs
```

## Common quality metrics

```text
Nonconformance
- open nonconformance count
- nonconformance aging
- repeat nonconformance count
- critical nonconformance count
- time to containment
- time to disposition
- time to closure

Hold
- active hold count
- hold aging
- inventory quantity on hold
- order hold count
- asset hold count
- release cycle time

CAPA
- open CAPA count
- overdue CAPA count
- CAPA aging
- ineffective CAPA count
- CAPA recurrence rate
- action completion rate

Supplier
- supplier quality issue count
- damaged receipt rate
- wrong item receipt rate
- supplier response time
- SCAR acceptance rate
- supplier repeat issue rate

Customer
- complaint count
- complaint response time
- complaint closure time
- repeat complaint rate
- customer rejection rate

Audit
- findings count
- major findings count
- repeat findings
- audit closure time
- finding closure time

Maintenance quality
- repeat repair count
- failed return-to-service count
- maintenance rework rate
- asset quality hold count

Inventory quality
- quarantine quantity
- expired stock count
- count-related quality issue count
- receiving discrepancy quality rate
```

## Quality risk profile

```text
QualityRiskProfile
- riskProfileId
- tenantId
- targetType
  - supplier
  - customer
  - process
  - site
  - asset
  - inventory_item
  - order
  - route
- targetRef
- riskLevel
  - low
  - moderate
  - high
  - critical
  - unknown
- riskFactors
- openIssueCount
- repeatIssueCount
- criticalIssueCount
- lastIncidentAt
- mitigationActions
- reviewedAt
- reviewedByPersonId
```

## Quality dashboard cards

```text
QualityDashboard
- Open nonconformances
- Critical nonconformances
- Active holds
- Hold aging
- Open CAPAs
- Overdue CAPAs
- CAPA effectiveness
- Supplier quality issues
- Customer complaint cases
- Audit findings
- Repeat issues
- Recently released holds
- Quality risk by site
- Quality risk by supplier
- Quality risk by process
```

## Quality status publishing workflow

```text
1. AssurArr creates or updates quality issue.
2. AssurArr recalculates target quality status.
3. AssurArr publishes QualityStatusSnapshot.
4. Target product consumes snapshot/event.
5. Target product blocks, warns, or allows workflow based on quality status and local rules.
6. ReportArr consumes quality facts for analytics.
```

## Supplier scorecard workflow

```text
1. AssurArr collects supplier quality issues, SCARs, holds, and nonconformances.
2. SupplyArr provides supplier context.
3. LoadArr provides receipt/discrepancy facts.
4. AssurArr calculates supplier quality scorecard.
5. SupplyArr consumes quality status/score for supplier decision support.
6. ReportArr displays supplier quality trends.
```

## Customer quality score workflow

```text
1. AssurArr collects customer complaint quality cases.
2. CustomArr provides customer context.
3. OrdArr/RoutArr/LoadArr provide fulfillment/delivery context.
4. AssurArr calculates customer quality metrics.
5. CustomArr receives quality activity/status.
6. ReportArr displays customer quality trends.
```

## Events

```text
assurarr.quality_status.changed
assurarr.quality_status.published
assurarr.scorecard.generated
assurarr.scorecard.reviewed
assurarr.risk_profile.updated
assurarr.metric.calculated
```
