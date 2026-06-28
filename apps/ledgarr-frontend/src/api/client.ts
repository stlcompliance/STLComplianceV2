const apiBase = import.meta.env.VITE_LEDGARR_API_BASE ?? ''

export interface LedgArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  launchableProductKeys: string[]
}

export interface LedgArrHandoffSessionResponse {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  launchableProductKeys: string[]
  themePreference?: string | null
  callbackUrl: string | null
}

type LegacyLedgArrSessionBootstrapPayload = LedgArrSessionBootstrapResponse & {
  hasLedgArrAccess?: boolean
}

type LegacyLedgArrHandoffSessionPayload = LedgArrHandoffSessionResponse & {
  launchableProductKeys?: string[]
}

function resolveLegacyLaunchableProductKeys(
  payload: { launchableProductKeys?: string[] },
): string[] {
  return payload.launchableProductKeys ?? []
}

export interface StlProductObjectReference {
  productKey: string
  objectType: string
  objectId: string
  objectNumber?: string | null
}

export interface LedgArrActivity {
  activityId: string
  action: string
  targetType: string
  targetId: string
  summary: string
  occurredAt: string
}

export interface LedgArrDashboardResponse {
  generatedAt: string
  financialLegalEntityCount: number
  openPacketCount: number
  postedJournalCount: number
  openPeriodCount: number
  closedPeriodCount: number
  openVendorBillCount: number
  openCustomerInvoiceCount: number
  postedDebitVolume: number
  openApAmount: number
  openArAmount: number
  recentActivity: LedgArrActivity[]
}

export interface FinancialPacket {
  id: string
  sourceProductKey: string
  sourceRecordDisplayName: string
  packetType: string
  accountingDate: string
  transactionCurrency: string
  sourceTotalAmount: number
  status: string
  receivedAt: string
}

export interface BillableEventSummary {
  id: string
  financialPacketId: string
  financialLegalEntityId: string | null
  financialLegalEntityDisplayName: string
  eventNumber: string
  sourceProductKey: string
  sourceRecordDisplayName: string
  chargeType: string
  customerRefId: string | null
  customerDisplayName: string
  amount: number
  currencyCode: string
  approvalStatus: string
  invoiceStatus: string
  holdReason: string | null
  exceptionReason: string | null
  accountingDate: string
}

export interface JournalEntry {
  id: string
  financialLegalEntityId?: string | null
  journalNumber: string
  status: string
  accountingDate: string
  description: string
  totalDebits: number
  totalCredits: number
  postedAt: string | null
}

export interface FinancialAuditEvent {
  id: string
  productKey: string
  action: string
  targetType: string
  targetId: string
  actorId: string
  summary: string
  reason: string | null
  occurredAt: string
}

export interface JournalAttachmentRef {
  id: string
  journalEntryId: string
  journalNumber: string
  recordArrDocumentId: string
  displayName: string
}

export interface JournalAuditTrail {
  id: string
  journalEntryId: string
  journalNumber: string
  action: string
  actorId: string
  summary: string
  occurredAt: string
}

export interface ApprovalStepSummary {
  stepNumber: number
  requiredPermissionKey: string
}

export interface ApprovalPolicySummary {
  id: string
  policyKey: string
  appliesTo: string
  requiresApproval: boolean
  steps: ApprovalStepSummary[]
}

export interface SegregationOfDutiesRule {
  id: string
  ruleKey: string
  incompatibleActions: string[]
}

export interface VendorBill {
  id: string
  vendorDisplayName: string
  vendorInvoiceNumber: string
  dueDate: string
  totalAmount: number
  status: string
  matchStatus: string
}

export interface CustomerInvoice {
  id: string
  customerDisplayName: string
  invoiceNumber: string
  dueDate: string
  totalAmount: number
  status: string
}

export interface FinancialLegalEntity {
  id: string
  entityCode: string
  displayName: string
  entityType: string
  baseCurrencyCode: string
  fiscalCalendarId?: string | null
  status: string
}

export interface FinancialLegalEntityRegistration {
  id: string
  financialLegalEntityId: string
  registrationType: string
  registrationNumber: string
  jurisdictionLabel: string
  status: string
}

export interface FinancialLegalEntityAddressSnapshot {
  id: string
  financialLegalEntityId: string
  snapshotLabel: string
  addressLine1: string
  city: string | null
  region: string | null
  postalCode: string | null
  countryCode: string
}

export interface FiscalPeriod {
  id: string
  financialLegalEntityId?: string | null
  periodKey: string
  name: string
  startDate: string
  endDate: string
  status: string
}

export interface PeriodActionPayload {
  reason: string
}

export interface GLAccount {
  id: string
  accountCode: string
  name: string
  accountType: string
  normalBalance: string
  status: string
}

export interface FinancialDimensionType {
  id: string
  dimensionKey: string
  displayName: string
}

export interface AgingBucket {
  bucket: string
  amount: number
}

export interface PaymentRunSummary {
  id: string
  paymentRunNumber: string
  status: string
  totalAmount: number
  latestExportedAt: string | null
  latestExportStatus: string | null
}

export interface CustomerPaymentSummary {
  id: string
  financialLegalEntityId: string
  customerRefId: string
  paymentNumber: string
  amount: number
  status: string
  appliedAmount: number
}

export interface FixedAssetSummary {
  id: string
  financialLegalEntityId: string
  maintainArrAssetRefId: string
  assetNumber: string
  assetClass: string
  inServiceDate: string
  capitalizedCost: number
  bookValue: number
  depreciationMethod: string
  usefulLifeMonths: number
  salvageValue: number
  status: string
  nextDepreciationDate: string | null
  remainingScheduleCount: number
}

export interface AssetDepreciationSchedule {
  id: string
  fixedAssetFinancialRecordId: string
  sequenceNumber: number
  depreciationDate: string
  depreciationAmount: number
  accumulatedDepreciation: number
  status: string
  postedJournalEntryId: string | null
}

export interface BudgetSummary {
  id: string
  financialLegalEntityId: string
  budgetNumber: string
  name: string
  status: string
  lineCount: number
  totalBudgetAmount: number
}

export interface FinancialProjectSummary {
  id: string
  financialLegalEntityId: string
  financialLegalEntityDisplayName: string
  projectCode: string
  name: string
  status: string
  budgetAmount: number
  actualCostAmount: number
  committedCostAmount: number
  allocatedCostAmount: number
  billingStatus: string
  taskCount: number
}

export interface TaxCode {
  id: string
  taxCodeKey: string
  displayName: string
  status: string
}

export interface TaxAdjustmentSummary {
  id: string
  financialLegalEntityId: string
  financialLegalEntityDisplayName: string
  taxCodeId: string
  taxCodeKey: string
  taxCodeDisplayName: string
  adjustmentNumber: string
  adjustmentDate: string
  amount: number
  currencyCode: string
  reason: string
  status: string
}

export interface TaxLiabilitySummary {
  financialLegalEntityId: string
  financialLegalEntityDisplayName: string
  taxCodeId: string
  taxCodeKey: string
  taxCodeDisplayName: string
  currencyCode: string
  liabilityAmount: number
  adjustmentCount: number
  latestAdjustmentDate: string
}

export interface FinancialLegalEntityRelationshipSummary {
  id: string
  parentFinancialLegalEntityId: string
  parentDisplayName: string
  childFinancialLegalEntityId: string
  childDisplayName: string
  relationshipType: string
  ownershipPercentage: number
  status: string
}

export interface IntercompanyTransactionSummary {
  id: string
  relationshipId: string
  fromFinancialLegalEntityId: string
  fromFinancialLegalEntityDisplayName: string
  toFinancialLegalEntityId: string
  toFinancialLegalEntityDisplayName: string
  transactionNumber: string
  transactionDate: string
  dueDate: string | null
  amount: number
  currencyCode: string
  description: string
  transactionType: string
  status: string
  settlementStatus: string
}

export interface IntercompanyBalanceSummary {
  fromFinancialLegalEntityId: string
  fromFinancialLegalEntityDisplayName: string
  toFinancialLegalEntityId: string
  toFinancialLegalEntityDisplayName: string
  currencyCode: string
  openAmount: number
  openTransactionCount: number
  oldestTransactionDate: string
  latestTransactionDate: string
}

export interface ExternalFinanceSystem {
  id: string
  systemKey: string
  displayName: string
  mode: string
  status: string
}

export interface ExternalPostingBatchSummary {
  id: string
  externalFinanceSystemId: string
  externalFinanceSystemDisplayName: string
  batchNumber: string
  status: string
  createdAt: string
  exportedAt: string | null
  journalCount: number
}

export interface BankAccountSummary {
  id: string
  financialLegalEntityId: string
  financialLegalEntityDisplayName: string
  bankName: string
  accountDisplayName: string
  accountType: string
  maskedAccountNumber: string
  currencyCode: string
  glCashAccountId: string
  glCashAccountCode: string
  status: string
  reconciliationEnabled: boolean
}

export interface BankTransactionSummary {
  id: string
  bankAccountId: string
  bankAccountDisplayName: string
  transactionDate: string
  description: string
  amount: number
  direction: string
  sourceType: string
  matchStatus: string
  purchaseOrderRefProductKey: string | null
  purchaseOrderRefType: string | null
  purchaseOrderRefId: string | null
  purchaseOrderRefDisplayName: string | null
  purchaseOrderApprovedAmountSnapshot: number | null
  purchaseOrderVarianceAmount: number | null
  purchaseOrderAmountStatus: string
  reconciliationStatus: string
  reconciliationId: string | null
}

export interface BankReconciliationSummary {
  id: string
  bankAccountId: string
  bankAccountDisplayName: string
  periodStartDate: string
  periodEndDate: string
  beginningBalance: number
  endingBalance: number
  statementDate: string
  clearedTransactionTotal: number
  adjustmentTotal: number
  matchedTransactionCount: number
  exceptionCount: number
  approvalStatus: string
  lockStatus: string
  status: string
}

export interface InventoryCostLayer {
  id: string
  financialLegalEntityId: string
  itemRefProductKey: string
  itemRefId: string
  sourceProductKey: string
  sourceRecordType: string
  sourceRecordId: string
  layerDate: string
  quantityOriginal: number
  quantityRemaining: number
  unitCost: number
  status: string
}

export interface PayrollCalendar {
  id: string
  legalEntityId: string
  name: string
  frequency: string
  periodStartDate: string
  periodEndDate: string
  payDate: string
  cutoffDate: string
  timezone: string
  status: string
}

export interface PayrollCodeMapping {
  id: string
  legalEntityId: string
  staffArrPayCodeRef: string
  payrollProviderRef: string | null
  providerEarningCode: string
  providerDeductionCode: string | null
  glAccountRef: string
  costCenterRef: string | null
  departmentRef: string | null
  taxableTreatmentSnapshot: string | null
  active: boolean
  effectiveStartDate: string
  effectiveEndDate: string | null
}

export interface PayrollBatch {
  id: string
  legalEntityId: string
  payrollCalendarId: string
  periodStartDate: string
  periodEndDate: string
  payDate: string
  status: string
  totalWorkers: number
  totalHours: number
  totalGrossEstimate: number | null
  exportProvider: string
  exportedAt: string | null
  approvedAt: string | null
  correctionReason: string | null
}

export interface PayrollBatchLine {
  id: string
  payrollBatchId: string
  personId: string
  workerNumber: string
  legalEntityId: string
  payrollCalendarId: string
  payCodeRef: string
  providerEarningCode: string
  durationMinutes: number
  rateSnapshot: number | null
  grossEstimate: number | null
  allocationSnapshot: string
  sourceTimesheetPeriodRef: string
  sourceTimeEntryRefs: string
  validationStatus: string
}

export interface PayrollExportPacket {
  id: string
  payrollBatchId: string
  providerKey: string
  exportFormat: string
  fileRef: string | null
  payloadHash: string
  exportedAt: string
  providerResponseStatus: string
  providerResponseRef: string | null
  errors: string | null
  replayProtectionKey: string
}

export interface PayrollJournalSnapshot {
  id: string
  payrollBatchId: string
  legalEntityId: string
  glAccountRef: string
  costCenterRef: string | null
  departmentRef: string | null
  productKey: string
  costObjectType: string
  costObjectRef: string
  debitAmount: number
  creditAmount: number
  currency: string
  sourcePayrollBatchLineRefs: string
  status: string
}

export interface TrialBalanceResponse {
  rows: TrialBalanceRow[]
  totalDebits: number
  totalCredits: number
}

export interface TrialBalanceRow {
  accountCode: string
  accountName: string
  debits: number
  credits: number
  balance: number
}

export interface ReportSummaryResponse {
  reportKey: string
  generatedAt: string
  totalDebits: number
  totalCredits: number
  mappingExceptionCount: number
}

export interface LedgArrSettingsSection {
  sectionKey: string
  displayName: string
  description: string
  rowVersion: string
  value: Record<string, unknown>
  highImpactFields: string[]
}

export interface LedgArrTenantSettingsEnvelope {
  settingsVersion: number
  sections: LedgArrSettingsSection[]
}

export interface LedgArrSettingsValidationResponse {
  isValid: boolean
  errors: Record<string, string[]>
}

export interface NamedOption {
  value: string
  label: string
}

export interface SettingsOptionReference {
  value: string
  label: string
  productKey: string
  objectType: string
  publicKey: string
  status: string
}

export interface LedgArrSettingsOptionsResponse {
  accounts: SettingsOptionReference[]
  legalEntities: SettingsOptionReference[]
  currencies: NamedOption[]
  sourceProducts: NamedOption[]
  sections: NamedOption[]
  crossProductReferences: SettingsOptionReference[]
}

export interface LedgArrSettingsAuditItem {
  auditId: string
  sectionKey: string
  changedAtUtc: string
  changedByPersonId: string
  changeReason: string | null
  diffJson: string | null
  correlationId: string | null
}

export interface LedgArrSettingsAuditResponse {
  items: LedgArrSettingsAuditItem[]
}

class LedgArrApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message)
    this.name = 'LedgArrApiError'
  }
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new LedgArrApiError(body || `${fallbackMessage} (${response.status})`, response.status)
  }

  return (await response.json()) as T
}

function normalizeLedgArrSessionBootstrapResponse(
  response: LegacyLedgArrSessionBootstrapPayload,
): LedgArrSessionBootstrapResponse {
  const { hasLedgArrAccess: _legacyHasLedgArrAccess, ...session } = response
  return {
    ...session,
    launchableProductKeys: resolveLegacyLaunchableProductKeys(session),
  }
}

function normalizeLedgArrHandoffSessionResponse(
  response: LegacyLedgArrHandoffSessionPayload,
): LedgArrHandoffSessionResponse {
  return {
    ...response,
    launchableProductKeys: resolveLegacyLaunchableProductKeys(response),
  }
}

export async function redeemHandoff(handoffCode: string): Promise<LedgArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/v1/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  const payload = await parseJsonResponse<LegacyLedgArrHandoffSessionPayload>(
    response,
    'Handoff redeem failed',
  )
  return normalizeLedgArrHandoffSessionResponse(payload)
}

export async function getSessionBootstrap(accessToken: string): Promise<LedgArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/v1/session`, {
    headers: authHeaders(accessToken),
  })
  const payload = await parseJsonResponse<LegacyLedgArrSessionBootstrapPayload>(
    response,
    'Failed to load session bootstrap',
  )
  return normalizeLedgArrSessionBootstrapResponse(payload)
}

export async function getDashboard(accessToken: string): Promise<LedgArrDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/dashboard`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LedgArrDashboardResponse>(response, 'Failed to load dashboard')
}

export async function listPackets(accessToken: string): Promise<FinancialPacket[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/financial-packets`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialPacket[]>(response, 'Failed to load packets')
}

export async function listBillableEvents(accessToken: string): Promise<BillableEventSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/billing/events`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BillableEventSummary[]>(response, 'Failed to load billable events')
}

export async function createBillableEventFromPacket(accessToken: string, packetId: string): Promise<BillableEventSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/billing/events/from-packet/${encodeURIComponent(packetId)}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BillableEventSummary>(response, 'Failed to create billable event from packet')
}

export async function approveBillableEvent(accessToken: string, billableEventId: string): Promise<BillableEventSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/billing/events/${encodeURIComponent(billableEventId)}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BillableEventSummary>(response, 'Failed to approve billable event')
}

export async function holdBillableEvent(
  accessToken: string,
  billableEventId: string,
  payload: { reason: string },
): Promise<BillableEventSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/billing/events/${encodeURIComponent(billableEventId)}/hold`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BillableEventSummary>(response, 'Failed to hold billable event')
}

export async function generateBillableEventInvoiceDraft(accessToken: string, billableEventId: string): Promise<BillableEventSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/billing/events/${encodeURIComponent(billableEventId)}/generate-invoice-draft`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BillableEventSummary>(response, 'Failed to generate invoice draft from billable event')
}

export async function listJournals(accessToken: string): Promise<{ journal: JournalEntry }[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/journals`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<{ journal: JournalEntry }[]>(response, 'Failed to load journals')
}

export async function listApprovalPolicies(accessToken: string): Promise<ApprovalPolicySummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/control/approval-policies`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ApprovalPolicySummary[]>(response, 'Failed to load approval policies')
}

export async function listSegregationOfDutiesRules(accessToken: string): Promise<SegregationOfDutiesRule[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/control/sod-rules`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SegregationOfDutiesRule[]>(response, 'Failed to load segregation of duties rules')
}

export async function listFinancialAuditEvents(
  accessToken: string,
  options?: { targetType?: string; take?: number },
): Promise<FinancialAuditEvent[]> {
  const search = new URLSearchParams()
  if (options?.targetType) {
    search.set('targetType', options.targetType)
  }
  if (typeof options?.take === 'number') {
    search.set('take', String(options.take))
  }
  const suffix = search.size ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/ledgarr/audit/events${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialAuditEvent[]>(response, 'Failed to load LedgArr audit events')
}

export async function listJournalAttachmentRefs(
  accessToken: string,
  journalEntryId?: string,
): Promise<JournalAttachmentRef[]> {
  const suffix = journalEntryId ? `?journalEntryId=${encodeURIComponent(journalEntryId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/ledgarr/journals/attachments${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<JournalAttachmentRef[]>(response, 'Failed to load journal document references')
}

export async function createJournalAttachmentRef(
  accessToken: string,
  journalEntryId: string,
  payload: { recordArrDocumentId: string; displayName: string },
): Promise<JournalAttachmentRef> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/journals/${encodeURIComponent(journalEntryId)}/attachments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<JournalAttachmentRef>(response, 'Failed to link RecordArr journal support')
}

export async function listJournalAuditTrail(
  accessToken: string,
  journalEntryId?: string,
): Promise<JournalAuditTrail[]> {
  const suffix = journalEntryId ? `?journalEntryId=${encodeURIComponent(journalEntryId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/ledgarr/journals/audit-trail${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<JournalAuditTrail[]>(response, 'Failed to load journal audit trail')
}

export async function listVendorBills(accessToken: string): Promise<VendorBill[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/ap/vendor-bills`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorBill[]>(response, 'Failed to load vendor bills')
}

export async function listCustomerInvoices(accessToken: string): Promise<CustomerInvoice[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/ar/customer-invoices`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomerInvoice[]>(response, 'Failed to load customer invoices')
}

export async function listFinancialLegalEntities(accessToken: string): Promise<FinancialLegalEntity[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/financial-legal-entities`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialLegalEntity[]>(response, 'Failed to load financial legal entities')
}

export async function listFinancialLegalEntityRegistrations(
  accessToken: string,
  financialLegalEntityId?: string,
): Promise<FinancialLegalEntityRegistration[]> {
  const suffix = financialLegalEntityId ? `?financialLegalEntityId=${encodeURIComponent(financialLegalEntityId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/ledgarr/financial-legal-entity-registrations${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialLegalEntityRegistration[]>(response, 'Failed to load legal entity registrations')
}

export async function listFinancialLegalEntityAddressSnapshots(
  accessToken: string,
  financialLegalEntityId?: string,
): Promise<FinancialLegalEntityAddressSnapshot[]> {
  const suffix = financialLegalEntityId ? `?financialLegalEntityId=${encodeURIComponent(financialLegalEntityId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/ledgarr/financial-legal-entity-addresses${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialLegalEntityAddressSnapshot[]>(response, 'Failed to load legal entity addresses')
}

export async function listFiscalPeriods(accessToken: string): Promise<FiscalPeriod[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/fiscal-periods`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FiscalPeriod[]>(response, 'Failed to load fiscal periods')
}

async function postFiscalPeriodAction(
  accessToken: string,
  fiscalPeriodId: string,
  action: 'close' | 'reopen' | 'lock',
  payload: PeriodActionPayload,
): Promise<FiscalPeriod> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/fiscal-periods/${encodeURIComponent(fiscalPeriodId)}/${action}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<FiscalPeriod>(response, `Failed to ${action} fiscal period`)
}

export async function closeFiscalPeriod(
  accessToken: string,
  fiscalPeriodId: string,
  payload: PeriodActionPayload,
): Promise<FiscalPeriod> {
  return postFiscalPeriodAction(accessToken, fiscalPeriodId, 'close', payload)
}

export async function reopenFiscalPeriod(
  accessToken: string,
  fiscalPeriodId: string,
  payload: PeriodActionPayload,
): Promise<FiscalPeriod> {
  return postFiscalPeriodAction(accessToken, fiscalPeriodId, 'reopen', payload)
}

export async function lockFiscalPeriod(
  accessToken: string,
  fiscalPeriodId: string,
  payload: PeriodActionPayload,
): Promise<FiscalPeriod> {
  return postFiscalPeriodAction(accessToken, fiscalPeriodId, 'lock', payload)
}

export async function listGLAccounts(accessToken: string): Promise<GLAccount[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/gl-accounts`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<GLAccount[]>(response, 'Failed to load general ledger accounts')
}

export async function listDimensionTypes(accessToken: string): Promise<FinancialDimensionType[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/dimensions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialDimensionType[]>(response, 'Failed to load financial dimensions')
}

export async function getApAging(accessToken: string): Promise<AgingBucket[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/ap/aging`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AgingBucket[]>(response, 'Failed to load AP aging')
}

export async function getArAging(accessToken: string): Promise<AgingBucket[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/ar/aging`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AgingBucket[]>(response, 'Failed to load AR aging')
}

export async function listPaymentRuns(accessToken: string): Promise<PaymentRunSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/ap/payment-runs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PaymentRunSummary[]>(response, 'Failed to load payment runs')
}

export async function listCustomerPayments(accessToken: string): Promise<CustomerPaymentSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/ar/customer-payments`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomerPaymentSummary[]>(response, 'Failed to load customer payments')
}

export async function listBudgets(accessToken: string): Promise<BudgetSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/budgets`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BudgetSummary[]>(response, 'Failed to load budgets')
}

export async function listFinancialProjects(accessToken: string): Promise<FinancialProjectSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/projects`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialProjectSummary[]>(response, 'Failed to load projects and jobs')
}

export async function listFixedAssets(accessToken: string): Promise<FixedAssetSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/fixed-assets`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FixedAssetSummary[]>(response, 'Failed to load fixed assets')
}

export async function listFixedAssetDepreciationSchedules(
  accessToken: string,
  fixedAssetId: string,
): Promise<AssetDepreciationSchedule[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/fixed-assets/${encodeURIComponent(fixedAssetId)}/depreciation-schedule`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetDepreciationSchedule[]>(response, 'Failed to load depreciation schedule')
}

export async function listTaxCodes(accessToken: string): Promise<TaxCode[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/tax/codes`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TaxCode[]>(response, 'Failed to load tax codes')
}

export async function listTaxAdjustments(accessToken: string): Promise<TaxAdjustmentSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/tax/adjustments`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TaxAdjustmentSummary[]>(response, 'Failed to load tax adjustments')
}

export async function listTaxLiabilitySummaries(accessToken: string): Promise<TaxLiabilitySummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/tax/liability-summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TaxLiabilitySummary[]>(response, 'Failed to load tax liability summary')
}

export async function createTaxAdjustment(
  accessToken: string,
  payload: {
    financialLegalEntityId: string
    taxCodeId: string
    adjustmentDate: string
    amount: number
    currencyCode?: string | null
    reason: string
    status?: string | null
  },
): Promise<TaxAdjustmentSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/tax/adjustments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TaxAdjustmentSummary>(response, 'Failed to create tax adjustment')
}

export async function listFinancialLegalEntityRelationships(
  accessToken: string,
): Promise<FinancialLegalEntityRelationshipSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/intercompany/relationships`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FinancialLegalEntityRelationshipSummary[]>(response, 'Failed to load intercompany relationships')
}

export async function listIntercompanyTransactions(accessToken: string): Promise<IntercompanyTransactionSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/intercompany/transactions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntercompanyTransactionSummary[]>(response, 'Failed to load intercompany transactions')
}

export async function listIntercompanyBalances(accessToken: string): Promise<IntercompanyBalanceSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/intercompany/balances`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntercompanyBalanceSummary[]>(response, 'Failed to load intercompany balances')
}

export async function createIntercompanyTransaction(
  accessToken: string,
  payload: {
    relationshipId: string
    fromFinancialLegalEntityId: string
    toFinancialLegalEntityId: string
    transactionDate: string
    dueDate?: string | null
    amount: number
    currencyCode?: string | null
    description: string
    transactionType?: string | null
    status?: string | null
    settlementStatus?: string | null
  },
): Promise<IntercompanyTransactionSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/intercompany/transactions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<IntercompanyTransactionSummary>(response, 'Failed to create intercompany transaction')
}

export async function settleIntercompanyTransaction(
  accessToken: string,
  transactionId: string,
  payload: { reason: string },
): Promise<IntercompanyTransactionSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/intercompany/transactions/${encodeURIComponent(transactionId)}/settle`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<IntercompanyTransactionSummary>(response, 'Failed to settle intercompany transaction')
}

export async function listExternalFinanceSystems(accessToken: string): Promise<ExternalFinanceSystem[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/external/systems`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalFinanceSystem[]>(response, 'Failed to load external finance systems')
}

export async function listExternalPostingBatches(accessToken: string): Promise<ExternalPostingBatchSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/external/posting-batches`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalPostingBatchSummary[]>(response, 'Failed to load external posting batches')
}

export async function listBankAccounts(accessToken: string): Promise<BankAccountSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/banking/accounts`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BankAccountSummary[]>(response, 'Failed to load bank accounts')
}

export async function createBankAccount(
  accessToken: string,
  payload: {
    financialLegalEntityId: string
    bankName: string
    accountDisplayName: string
    accountType?: string | null
    maskedAccountNumber: string
    currencyCode?: string | null
    glCashAccountId: string
    reconciliationEnabled: boolean
  },
): Promise<BankAccountSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/banking/accounts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BankAccountSummary>(response, 'Failed to create bank account')
}

export async function listBankTransactions(accessToken: string, bankAccountId?: string): Promise<BankTransactionSummary[]> {
  const suffix = bankAccountId ? `?bankAccountId=${encodeURIComponent(bankAccountId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/ledgarr/banking/transactions${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BankTransactionSummary[]>(response, 'Failed to load bank transactions')
}

export async function createBankTransaction(
  accessToken: string,
  payload: {
    bankAccountId: string
    transactionDate: string
    description: string
    amount: number
    direction?: string | null
    sourceType?: string | null
    matchStatus?: string | null
    purchaseOrderRef?: StlProductObjectReference | null
    purchaseOrderApprovedAmountSnapshot?: number | null
  },
): Promise<BankTransactionSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/banking/transactions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BankTransactionSummary>(response, 'Failed to create bank transaction')
}

export async function listBankReconciliations(accessToken: string): Promise<BankReconciliationSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/banking/reconciliations`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BankReconciliationSummary[]>(response, 'Failed to load bank reconciliations')
}

export async function listInventoryCostLayers(accessToken: string): Promise<InventoryCostLayer[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/inventory-valuation/layers`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InventoryCostLayer[]>(response, 'Failed to load inventory cost layers')
}

export async function createBankReconciliation(
  accessToken: string,
  payload: {
    bankAccountId: string
    periodStartDate: string
    periodEndDate: string
    beginningBalance: number
    endingBalance: number
    statementDate: string
    adjustmentTotal: number
    bankTransactionIds: string[]
  },
): Promise<BankReconciliationSummary> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/banking/reconciliations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BankReconciliationSummary>(response, 'Failed to create bank reconciliation')
}

export async function getTrialBalance(accessToken: string): Promise<TrialBalanceResponse> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/reports/trial-balance`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrialBalanceResponse>(response, 'Failed to load trial balance')
}

export async function getReportSummary(accessToken: string, reportKey: string): Promise<ReportSummaryResponse> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/reports/${encodeURIComponent(reportKey)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReportSummaryResponse>(response, `Failed to load ${reportKey} report summary`)
}

export async function listPayrollCalendars(accessToken: string): Promise<PayrollCalendar[]> {
  const response = await fetch(`${apiBase}/api/v1/payroll/calendars`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PayrollCalendar[]>(response, 'Failed to load payroll calendars')
}

export async function listPayrollCodeMappings(accessToken: string): Promise<PayrollCodeMapping[]> {
  const response = await fetch(`${apiBase}/api/v1/payroll/code-mappings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PayrollCodeMapping[]>(response, 'Failed to load payroll code mappings')
}

export async function listPayrollBatches(accessToken: string): Promise<PayrollBatch[]> {
  const response = await fetch(`${apiBase}/api/v1/payroll/batches`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PayrollBatch[]>(response, 'Failed to load payroll batches')
}

export async function listPayrollBatchLines(accessToken: string, batchId: string): Promise<PayrollBatchLine[]> {
  const response = await fetch(`${apiBase}/api/v1/payroll/batches/${encodeURIComponent(batchId)}/lines`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PayrollBatchLine[]>(response, 'Failed to load payroll batch lines')
}

export async function listPayrollExportPackets(accessToken: string, batchId: string): Promise<PayrollExportPacket[]> {
  const response = await fetch(`${apiBase}/api/v1/payroll/batches/${encodeURIComponent(batchId)}/export-packets`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PayrollExportPacket[]>(response, 'Failed to load payroll export packets')
}

export async function listPayrollJournalSnapshots(accessToken: string, batchId: string): Promise<PayrollJournalSnapshot[]> {
  const response = await fetch(`${apiBase}/api/v1/payroll/batches/${encodeURIComponent(batchId)}/journal-snapshot`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PayrollJournalSnapshot[]>(response, 'Failed to load payroll journal snapshots')
}

export async function getLedgArrTenantSettings(accessToken: string): Promise<LedgArrTenantSettingsEnvelope> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LedgArrTenantSettingsEnvelope>(response, 'Failed to load LedgArr tenant settings')
}

export async function getLedgArrTenantSettingsSection(accessToken: string, sectionKey: string): Promise<LedgArrSettingsSection> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/settings/${encodeURIComponent(sectionKey)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LedgArrSettingsSection>(response, `Failed to load ${sectionKey} settings`)
}

export async function updateLedgArrTenantSettingsSection(
  accessToken: string,
  sectionKey: string,
  payload: { value: Record<string, unknown>; expectedRowVersion?: string | null; reason?: string | null },
): Promise<LedgArrSettingsSection> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/settings/${encodeURIComponent(sectionKey)}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({
      value: payload.value,
      expectedRowVersion: payload.expectedRowVersion ?? null,
      reason: payload.reason ?? null,
    }),
  })
  return parseJsonResponse<LedgArrSettingsSection>(response, `Failed to update ${sectionKey} settings`)
}

export async function validateLedgArrTenantSettingsSection(
  accessToken: string,
  sectionKey: string,
  payload: { value: Record<string, unknown>; expectedRowVersion?: string | null; reason?: string | null },
): Promise<LedgArrSettingsValidationResponse> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/settings/${encodeURIComponent(sectionKey)}/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({
      value: payload.value,
      expectedRowVersion: payload.expectedRowVersion ?? null,
      reason: payload.reason ?? null,
    }),
  })
  return parseJsonResponse<LedgArrSettingsValidationResponse>(response, `Failed to validate ${sectionKey} settings`)
}

export async function resetLedgArrTenantSettingsSection(
  accessToken: string,
  sectionKey: string,
  reason?: string | null,
): Promise<LedgArrSettingsSection> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/settings/${encodeURIComponent(sectionKey)}/reset`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ reason: reason ?? null }),
  })
  return parseJsonResponse<LedgArrSettingsSection>(response, `Failed to reset ${sectionKey} settings`)
}

export async function getLedgArrTenantSettingsAudit(accessToken: string, sectionKey: string): Promise<LedgArrSettingsAuditResponse> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/settings/${encodeURIComponent(sectionKey)}/audit`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LedgArrSettingsAuditResponse>(response, `Failed to load ${sectionKey} settings audit`)
}

export async function getLedgArrTenantSettingsOptions(accessToken: string): Promise<LedgArrSettingsOptionsResponse> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/settings/options`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LedgArrSettingsOptionsResponse>(response, 'Failed to load LedgArr settings options')
}
