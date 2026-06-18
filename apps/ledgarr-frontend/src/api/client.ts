const apiBase = import.meta.env.VITE_LEDGARR_API_BASE ?? ''

export interface LedgArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasLedgArrEntitlement: boolean
  entitlements: string[]
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
  entitlements: string[]
  themePreference?: string | null
  callbackUrl: string | null
}

export interface LedgArrActivity {
  activityId: string
  action: string
  targetType: string
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

export interface JournalEntry {
  id: string
  journalNumber: string
  status: string
  accountingDate: string
  description: string
  totalDebits: number
  totalCredits: number
  postedAt: string | null
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

export async function redeemHandoff(handoffCode: string): Promise<LedgArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/v1/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<LedgArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getSessionBootstrap(accessToken: string): Promise<LedgArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/v1/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LedgArrSessionBootstrapResponse>(response, 'Failed to load session bootstrap')
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

export async function listJournals(accessToken: string): Promise<{ journal: JournalEntry }[]> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/journals`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<{ journal: JournalEntry }[]>(response, 'Failed to load journals')
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

export async function getTrialBalance(accessToken: string): Promise<TrialBalanceResponse> {
  const response = await fetch(`${apiBase}/api/v1/ledgarr/reports/trial-balance`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrialBalanceResponse>(response, 'Failed to load trial balance')
}
