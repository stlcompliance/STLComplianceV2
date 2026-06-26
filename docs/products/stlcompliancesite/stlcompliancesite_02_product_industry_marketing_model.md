# STLComplianceSite — Product, Industry, and Marketing Model

## Product marketing page

A ProductMarketingPage explains one product in the suite without turning the public site into product documentation.

```text
ProductMarketingPage
- productMarketingPageId
- productKey
  - nexarr
  - staffarr
  - trainarr
  - compliancecore
  - maintainarr
  - loadarr
  - supplyarr
  - routarr
  - customarr
  - ordarr
  - recordarr
  - assurarr
  - reportarr
  - fieldcompanion
- pageId
- productName
- productTagline
- oneLiner
- status
  - draft
  - review
  - published
  - archived
- audience
- painPoints
- capabilityGroups
- crossProductConnections
- workflowExamples
- screenshotsOrMockups
- faqRefs
- callToActionRefs
- seoMetadataRef
```

## Product capability group

```text
ProductCapabilityGroup
- capabilityGroupId
- productMarketingPageId
- title
- description
- capabilityItems
- sortOrder
```

## Product capability item

```text
ProductCapabilityItem
- capabilityItemId
- capabilityGroupId
- title
- description
- relatedProductKeys
- relatedWorkflowRefs
- status
```

## Product connection

ProductConnection explains how one product connects to another.

```text
ProductConnection
- productConnectionId
- sourceProductKey
- targetProductKey
- connectionTitle
- description
- exampleFlow
- sourceOwns
- targetOwns
- publicSummary
- status
```

## Product page recommended one-liners

```text
NexArr
- The secure front door for the suite.

StaffArr
- The people, permissions, and internal location backbone.

TrainArr
- The LMS and qualification engine.

Compliance Core
- The rule and requirement brain.

MaintainArr
- The maintenance execution system.

LoadArr
- The WMS and inventory execution system.

SupplyArr
- The supplier and procurement execution system.

RoutArr
- The dispatch and transportation execution system.

CustomArr
- The customer relationship and customer requirement system.

OrdArr
- The order and request coordination system.

RecordArr
- The document, evidence, and retention vault.

AssurArr
- The quality, hold, nonconformance, and CAPA system.

ReportArr
- The reporting and analytics layer.

Field Companion
- The mobile execution interface for field work.
```

## Industry page

An IndustryPage explains how the suite applies to a market/operation type.

```text
IndustryPage
- industryPageId
- pageId
- industryKey
  - fleet
  - warehousing
  - manufacturing
  - transportation
  - maintenance
  - safety_compliance
  - regulated_operations
  - field_service
  - logistics
  - mixed_operations
- title
- description
- painPoints
- solutionSummary
- relevantProductKeys
- workflowExamples
- complianceExamples
- callToActionRefs
- status
```

## Use case page

```text
UseCasePage
- useCasePageId
- pageId
- useCaseKey
  - asset_maintenance
  - training_readiness
  - audit_evidence
  - warehouse_receiving
  - supplier_compliance
  - route_delivery
  - quality_hold
  - customer_order_fulfillment
  - incident_retraining
  - document_control
  - mobile_field_execution
- title
- problemStatement
- solutionSummary
- involvedProductKeys
- stepByStepFlow
- outcomeSummary
- evidenceGenerated
- ctaRefs
- status
```

## Marketing workflow example

```text
MarketingWorkflowExample
- workflowExampleId
- title
- description
- involvedProductKeys
- steps
- sourceOfTruthSummary
- publicDiagramRef
- status
```

## Competitive/comparison page

```text
ComparisonPage
- comparisonPageId
- pageId
- comparisonType
  - fragmented_stack
  - point_solution
  - spreadsheet_manual
  - legacy_system
  - generic
- title
- comparisonRows
- positioningSummary
- callToActionRefs
- status
```

## FAQ item

```text
FaqItem
- faqItemId
- pageId
- question
- answer
- category
  - platform
  - pricing
  - security
  - products
  - compliance
  - implementation
  - data
  - support
- status
  - draft
  - published
  - archived
- sortOrder
```

## Product page template

```text
ProductPageTemplate
- Hero
  - Product name
  - One-liner
  - Practical description
- Problem
  - What breaks without this product
- Capabilities
  - 3 to 6 capability groups
- Cross-product connections
  - What this product owns
  - What it consumes from others
  - What it emits to others
- Workflow example
  - Concrete real-world flow
- Evidence/reporting
  - What records, audit events, and reports it supports
- CTA
  - Request demo/contact/explore suite
```

## Industry page template

```text
IndustryPageTemplate
- Hero
- Industry pain points
- Connected STL Compliance solution
- Relevant product workspaces
- Example workflows
- Compliance/evidence angle
- Why connected suite beats fragmented tools
- CTA
```

## Public suite flow examples

```text
New hire to qualified worker
- StaffArr creates person.
- TrainArr assigns onboarding.
- RecordArr stores evidence.
- ReportArr shows readiness.

Work order needs parts
- MaintainArr creates demand.
- LoadArr checks inventory.
- SupplyArr purchases if needed.
- RecordArr stores evidence.
- ReportArr shows delays.

Receiving with BOL capture
- LoadArr receives.
- Field Companion captures BOL.
- RecordArr scans/OCRs.
- AssurArr handles discrepancy.
- SupplyArr sees supplier impact.

Route delivery proof
- RoutArr executes trip.
- Field Companion captures POD.
- RecordArr stores proof.
- OrdArr closes delivery dependency.
- CustomArr sees customer activity.

Quality hold blocks fulfillment
- AssurArr places hold.
- LoadArr/OrdArr/RoutArr/MaintainArr obey hold.
- RecordArr stores evidence.
- ReportArr shows quality impact.
```

## Marketing content workflow

```text
1. Product/industry/use-case page is drafted.
2. Product ownership wording is checked against suite constitution.
3. Content is reviewed for overclaiming.
4. SEO and CTA metadata are added.
5. Page is published.
6. Analytics events track CTA and engagement.
7. Content is revised as product lineup evolves.
```

## Events

```text
site.product_page.created
site.product_page.updated
site.product_page.published
site.industry_page.created
site.industry_page.published
site.use_case_page.created
site.use_case_page.published
site.workflow_example.created
site.faq.created
site.comparison_page.published
```
