# SupplyArr — Supplier Compliance, Quality, and Performance Model

## Supplier compliance requirement

A SupplierComplianceRequirement tracks a required supplier document, certification, insurance, license, contract, tax document, safety document, or quality document.

Compliance Core owns the requirement meaning. RecordArr owns the document file. SupplyArr owns the supplier-side requirement tracking status.

```text
SupplierComplianceRequirement
- supplierComplianceRequirementId
- tenantId
- supplierId
- requirementNumber
- requirementType
  - insurance
  - certification
  - contract
  - license
  - tax_document
  - safety_document
  - quality_document
  - compliance_document
  - banking_reference
  - other
- title
- description
- complianceCoreRequirementRef
- evidenceTypeRef
- required
- status
  - missing
  - requested
  - submitted
  - under_review
  - accepted
  - rejected
  - expired
  - waived
  - not_applicable
- recordRefs
- requestedAt
- submittedAt
- reviewedByPersonId
- reviewedAt
- expiresAt
- waiverReason
- rejectionReason
- notes
```

## Supplier compliance status snapshot

```text
SupplierComplianceStatusSnapshot
- complianceStatusSnapshotId
- tenantId
- supplierId
- overallStatus
  - compliant
  - warning
  - noncompliant
  - missing_documents
  - expired_documents
  - unknown
- missingRequirementCount
- expiredRequirementCount
- rejectedRequirementCount
- warningCount
- lastEvaluatedAt
- complianceCoreEvaluationRef
```

## Supplier quality status snapshot

AssurArr owns quality hold/nonconformance decisions. SupplyArr stores/uses a snapshot.

```text
SupplierQualityStatusSnapshot
- qualityStatusSnapshotId
- tenantId
- supplierId
- overallStatus
  - acceptable
  - warning
  - on_hold
  - blocked
  - unknown
- activeHoldRefs
- openNonconformanceRefs
- openScarRefs
- repeatIssueCount
- lastQualityIssueAt
- lastResolvedAt
- sourceProduct: assurarr
```

## Supplier performance record

```text
SupplierPerformanceRecord
- performanceRecordId
- tenantId
- supplierId
- periodStart
- periodEnd
- status
  - draft
  - calculated
  - reviewed
  - archived
- onTimeDeliveryRate
- averageLeadTimeDays
- leadTimeVarianceDays
- receiptDiscrepancyCount
- qualityIssueCount
- nonconformanceRefs
- scarRefs
- lateDeliveryCount
- emergencyPurchaseCount
- priceVariancePercent
- complianceIssueCount
- responseTimeHours
- overallScore
- performanceStatus
  - excellent
  - acceptable
  - warning
  - poor
  - blocked
  - unknown
- generatedAt
- reviewedByPersonId
- reviewedAt
```

## Supplier issue

SupplyArr may track procurement-facing supplier issues. Quality issues should route to AssurArr.

```text
SupplierIssue
- supplierIssueId
- tenantId
- issueNumber
- supplierId
- issueType
  - late_delivery
  - no_response
  - pricing_dispute
  - missing_document
  - compliance_issue
  - quality_issue
  - wrong_item
  - damaged_goods
  - service_issue
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - investigating
  - waiting_supplier
  - escalated_to_assurarr
  - resolved
  - closed
  - canceled
- sourceProduct
- sourceObjectRef
- purchaseOrderRef
- receiptRef
- ownerPersonId
- recordRefs
- assurarrNonconformanceRef
- resolutionSummary
- openedAt
- closedAt
```

## Supplier communication

```text
SupplierCommunication
- communicationId
- tenantId
- supplierId
- communicationType
  - email
  - phone
  - meeting
  - portal_message
  - document_request
  - quote_request
  - po_sent
  - complaint
  - corrective_action
- direction
  - inbound
  - outbound
  - internal_note
- subject
- summary
- contactRef
- personId
- sourceProduct
- sourceObjectRef
- recordRefs
- occurredAt
```

## Supplier document request

```text
SupplierDocumentRequest
- documentRequestId
- tenantId
- supplierId
- requirementRef
- requestedDocumentType
- status
  - draft
  - sent
  - viewed
  - submitted
  - accepted
  - rejected
  - expired
  - canceled
- requestedByPersonId
- requestedAt
- dueAt
- secureUploadSessionRef
- submittedRecordRefs
- reviewedByPersonId
- reviewedAt
```

## Compliance workflow

```text
1. Supplier is created or updated.
2. SupplyArr determines required documents based on supplier type, services, items, and Compliance Core requirements.
3. SupplierComplianceRequirements are created.
4. Supplier documents are requested.
5. RecordArr stores submitted documents.
6. Compliance Core evaluates evidence where applicable.
7. Reviewer accepts/rejects/waives requirements.
8. Supplier compliance status snapshot updates.
9. Supplier approval/restriction/block status may change.
```

## Supplier quality workflow

```text
1. LoadArr or AssurArr reports quality issue.
2. SupplyArr records supplier issue/performance impact.
3. AssurArr owns nonconformance, quality hold, and SCAR.
4. SupplyArr updates supplier quality status snapshot.
5. Supplier may become restricted/suspended/blocked based on policy.
```

## Supplier performance workflow

```text
1. LoadArr sends receipt/discrepancy facts.
2. SupplyArr sends PO/lead time facts.
3. AssurArr sends quality issue/SCAR facts.
4. SupplyArr calculates SupplierPerformanceRecord.
5. Buyer reviews scorecard.
6. Supplier status/preferred flag/restrictions may be updated.
7. ReportArr consumes supplier performance metrics.
```

## Supplier document request workflow

```text
1. Buyer/compliance user requests supplier document.
2. SupplyArr creates SupplierDocumentRequest.
3. RecordArr/Field Companion secure upload session may be created.
4. Supplier submits document.
5. RecordArr stores document.
6. SupplyArr routes review.
7. Requirement is accepted/rejected/expired.
```

## Events

```text
supplyarr.supplier_compliance_requirement.created
supplyarr.supplier_compliance_requirement.requested
supplyarr.supplier_compliance_requirement.submitted
supplyarr.supplier_compliance_requirement.accepted
supplyarr.supplier_compliance_requirement.rejected
supplyarr.supplier_compliance_requirement.expired
supplyarr.supplier_compliance_requirement.waived

supplyarr.supplier_compliance_status.changed
supplyarr.supplier_quality_status.changed

supplyarr.supplier_performance.calculated
supplyarr.supplier_performance.reviewed

supplyarr.supplier_issue.created
supplyarr.supplier_issue.escalated_to_assurarr
supplyarr.supplier_issue.resolved
supplyarr.supplier_issue.closed

supplyarr.supplier_document_request.created
supplyarr.supplier_document_request.sent
supplyarr.supplier_document_request.submitted
supplyarr.supplier_document_request.accepted
supplyarr.supplier_document_request.rejected

supplyarr.supplier_communication.created
```
