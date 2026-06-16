# OrdArr - Completion and Financial Packet Model

OrdArr coordinates completion and prepares operational handoff packets for external financial execution.

OrdArr does not create invoices, bills, payments, tax records, ledger entries, or accounting close records.

## CompletionPacket

CompletionPacket summarizes the operational completion state for an order/request.

Fields:

- completionPacketId
- orderRequestId
- tenantId
- status
  - draft
  - assembling
  - missing_evidence
  - ready_for_review
  - approved
  - rejected
  - stored
  - superseded
- packetType
  - order_completion
  - service_completion
  - delivery_completion
  - maintenance_completion
  - fulfillment_completion
  - mixed_work_completion
- sourceProductRefs
- completedWorkRefs
- proofRefs
- recordPackageRef
- evidenceRequirementRefs
- missingEvidenceSummary
- reviewedByPersonRef
- reviewedAt
- storedRecordRef
- createdAt
- updatedAt

RecordArr owns the stored packet files, record package, retention, access history, and controlled document lifecycle.

## CompletionFact

CompletionFact captures source-product facts used in closeout.

Fields:

- completionFactId
- completionPacketId
- sourceProductKey
- sourceRecordRef
- factType
  - fulfillment_completed
  - delivery_completed
  - work_order_closed
  - procurement_completed
  - quality_release_confirmed
  - evidence_attached
  - customer_signoff_received
  - exception_resolved
- factStatus
  - confirmed
  - missing
  - stale
  - disputed
  - not_applicable
- factSnapshot
- snapshotAt
- freshness
- sourceEventRef

## InvoiceReadyPacket

InvoiceReadyPacket prepares operational billing context for an external finance system.

Fields:

- invoiceReadyPacketId
- orderRequestId
- completionPacketId
- tenantId
- status
  - draft
  - assembling
  - pending_review
  - ready
  - sent
  - accepted_by_external_system
  - rejected_by_external_system
  - cancelled
- billToCustomerRef
- customerMappingRef
- serviceSummary
- lineSummaries
- operationalAmountSnapshot
- taxHandlingNote
- supportingRecordRefs
- externalFinanceSystem
- externalInvoiceRef
- externalStatusSnapshot
- lastSentAt
- lastError

External finance systems own invoices, payment, tax, receivables, ledger, and accounting close.

## BillReadyPacket

BillReadyPacket prepares operational payable context for an external finance system.

Fields:

- billReadyPacketId
- orderRequestId
- sourceProcurementRefs
- completionPacketId
- tenantId
- status
  - draft
  - assembling
  - pending_review
  - ready
  - sent
  - accepted_by_external_system
  - rejected_by_external_system
  - cancelled
- supplierRef
- supplierMappingRef
- purchaseSummary
- receiptRefs
- operationalCostSnapshot
- supportingRecordRefs
- externalFinanceSystem
- externalBillRef
- externalStatusSnapshot
- lastSentAt
- lastError

SupplyArr owns supplier/procurement context. LoadArr owns receiving and stock movement truth. External finance systems own bills, payments, tax, payables, ledger, and accounting close.

## CloseoutChecklist

CloseoutChecklist tracks whether an order/request can be closed.

Fields:

- closeoutChecklistId
- orderRequestId
- status
  - open
  - blocked
  - ready
  - completed
- requiredItems
- missingItems
- blockerRefs
- lastEvaluatedAt
- evaluatedBy

Common required items:

- Target handoffs completed or cancelled with reason.
- Required proofs and documents stored in RecordArr.
- Compliance evidence requirements satisfied or explicitly waived.
- Quality holds/releases resolved by AssurArr where applicable.
- Customer signoff captured where required.
- Invoice-ready and bill-ready packet decisions recorded.

## Packet events

- `ordarr.completion_packet.assembling`
- `ordarr.completion_packet.ready_for_review`
- `ordarr.completion_packet.approved`
- `ordarr.completion_packet.stored`
- `ordarr.invoice_ready_packet.ready`
- `ordarr.invoice_ready_packet.sent`
- `ordarr.invoice_ready_packet.external_status_updated`
- `ordarr.bill_ready_packet.ready`
- `ordarr.bill_ready_packet.sent`
- `ordarr.bill_ready_packet.external_status_updated`
- `ordarr.closeout_checklist.blocked`
- `ordarr.closeout_checklist.ready`
- `ordarr.closeout_checklist.completed`
