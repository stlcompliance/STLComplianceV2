# STL Compliance External Systems Integration Constitution

## 1. Purpose

This constitution defines how STL Compliance integrates with external systems while preserving STL ownership boundaries and keeping finance, payroll, banking, tax, certified hardware, and specialized vendor systems external unless STL explicitly builds a replacement product.

## 2. Scope

This constitution applies to integrations with:

- QuickBooks
- ERP/accounting systems
- Payroll systems
- HRIS systems
- ELD systems
- Telematics systems
- Certified hardware systems
- Supplier APIs
- Carrier APIs
- CRM systems
- Government/public APIs
- External document systems
- Banking, tax, and payment systems

## 3. Prime directive

External systems remain external unless STL explicitly builds a replacement product.

STL may integrate, map, consume, snapshot, classify, report, and hand off.

STL must not silently become the system of record for external domains it does not own.

## 4. External ownership examples

QuickBooks/ERP owns:

- Invoices
- Bills
- Payments
- Accounts payable
- Accounts receivable
- Tax
- General ledger
- Bank reconciliation
- Accounting close

Payroll owns:

- Payroll execution
- Tax withholding
- Direct deposit
- Wage statements
- Payroll filings

ELD/telematics/hardware vendors own:

- Certified ELD capture
- Hardware-generated records
- Device telemetry
- Firmware/hardware lifecycle
- Hardware compliance certifications

External CRM may own:

- External sales pipeline
- Marketing automation
- CRM-specific workflows

## 5. STL ownership around integrations

STL may own:

- Operational customer/vendor records
- Completion packets
- Invoice-ready packets
- Bill-ready packets
- External ID mappings
- External status snapshots
- Operational use of external hardware data
- Evidence classification
- Related work/status decisions
- Sync status and integration health

## 6. Integration direction

Every integration must declare direction:

- `inbound`
- `outbound`
- `bidirectional`
- `read_only`
- `writeback`

Bidirectional integrations require conflict rules.

Writeback integrations require idempotency, reviewability, and audit.

## 7. External mappings

External mappings must include:

- STL tenant ID
- STL owning product
- STL entity type
- STL entity ID
- External system
- External entity type
- External ID
- Mapping status
- Sync direction
- Last verified time
- Last sync time
- Last error when applicable

External IDs must not replace STL canonical IDs unless the ownership constitution explicitly says the external system is the source of truth.

## 8. Snapshots

External status snapshots must be labeled as snapshots.

Examples:

- QuickBooks invoice status snapshot
- Payroll export status snapshot
- ELD hours-of-service status snapshot
- Supplier order status snapshot
- Carrier delivery status snapshot

Snapshots should include:

- External system
- External ID
- Snapshot time
- Status
- Source payload version/hash where appropriate
- Freshness

## 9. External credentials

External credentials/tokens must be managed through approved secure storage and integration configuration.

Credentials must be:

- Tenant-scoped or platform-scoped as appropriate
- Encrypted/secret-managed
- Permission-protected
- Rotatable
- Auditable when changed
- Not exposed to ordinary users

## 10. Financial integration rules

STL may prepare financial handoff packets.

STL must not own invoices, bills, payments, tax, ledger, or accounting close unless a future ownership constitution explicitly creates that product/domain.

Financial handoff packets should include:

- Source product(s)
- Operational completion summary
- Customer/vendor mapping
- Amount/cost/revenue snapshot when operationally derived
- Supporting documents
- Approval status
- External system target
- Handoff status

External finance system response becomes an external status snapshot.

## 11. ELD/telematics/hardware rules

STL must not pretend phone/mobile workflows replace certified hardware where certified hardware is required.

STL may consume:

- HOS status
- Vehicle telemetry
- GPS/location events
- Fault codes
- Driver logs
- Device health

STL must show source and freshness when external hardware data affects dispatch, compliance, maintenance, or reporting decisions.

RoutArr visibility ingestion may normalize ELD, telematics, carrier API, GPS, and manual check-call events into transportation visibility snapshots, but those snapshots must remain labeled as external or calculated operational signals.

## 12. Supplier and parts provider rules

Supplier integrations may provide:

- Catalog data
- Availability
- Price snapshots
- Lead time snapshots
- Order status
- Shipment status
- Invoices/bills handoff metadata

SupplyArr owns supplier/item/procurement context inside STL.

LoadArr owns receiving/inventory movement.

QuickBooks/ERP owns financial execution.

## 13. External writebacks

External writebacks must be:

- Explicit
- Idempotent
- Tenant-scoped
- Permission/service-token scoped
- Audited
- Retry-safe
- Failure-visible

No external writeback may occur as an undocumented side effect.

## 14. Sync failures

Integration failure states must show:

- External system
- Affected tenant/product/record
- Last successful sync
- Last failed sync
- Error category
- Retryable/manual review state
- Next retry when applicable
- Business impact

Failure must not silently corrupt STL source records.

## 15. Import from external systems

Inbound external data must be classified as:

- Canonical external truth
- Candidate reference data
- Tenant operational import
- Snapshot
- Evidence
- Mapping candidate

Imported external data must not bypass staging/review where ambiguity exists.

## 16. Anti-patterns

The following are not allowed:

- Treating QuickBooks status as STL invoice ownership
- Treating ELD/telematics data as STL hardware ownership
- Using external IDs as STL canonical IDs without explicit decision
- Silent external writebacks
- Credentials stored in product code or frontend config
- Sync failures hidden from users/admins
- Bidirectional sync without conflict rules
- External data overwriting product-owned truth without validation
- Mobile app pretending to be certified hardware

## 17. Minimum acceptable implementation

An external integration is minimally acceptable when it has:

1. Declared external system and ownership boundary
2. Sync direction
3. External ID mapping
4. Credential/security model
5. Source/freshness metadata
6. Idempotent writeback where applicable
7. Failure visibility
8. Audit/activity for material actions
9. No silent ownership transfer
