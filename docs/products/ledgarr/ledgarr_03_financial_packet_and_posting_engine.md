# LedgArr 03 - Financial Packet and Posting Engine

Financial packets are the canonical contract from operational products to LedgArr. They are not ledger entries until LedgArr validates, maps, previews, approves, and posts them.

Required packet identity:

- tenantId
- financialLegalEntityId or resolution input
- sourceProductKey
- sourceEventId
- sourceEventVersion
- sourceRecordType
- sourceRecordId
- sourceRecordDisplayName
- sourceOccurredAt
- packetType and packetSubType
- accountingDate
- transactionCurrency
- sourceAmount, sourceTaxAmount, sourceTotalAmount
- lines, sourceRefs, dimensionHints, documentRefs, approvalHints
- idempotencyKey

Required packet lifecycle:

1. received
2. validation_failed or needs_mapping when required
3. mapped
4. preview_ready
5. pending_approval when policy requires approval
6. approved
7. posted, rejected, superseded, voided, or failed_posting

Posting entities:

- PostingRule, PostingRuleLine, PostingRuleCondition, PostingRuleVersion
- PostingPreview and PostingPreviewLine
- PostingBatch and PostingBatchLine
- JournalEntry and JournalLine
- JournalEntryReversal
- JournalAttachmentRef
- JournalApproval
- JournalAuditTrail

Posting rules:

- Ingestion is idempotent by tenantId, sourceProductKey, sourceEventId, and sourceEventVersion.
- Every JournalEntry needs at least two JournalLines.
- Debits must equal credits before posting.
- Posted JournalEntry and JournalLine financial amounts are immutable.
- Reversal entries must reference the original JournalEntry.
- Missing fiscal period, closed periods, locked periods, missing required dimensions, and missing FinancialLegalEntity must block posting.
